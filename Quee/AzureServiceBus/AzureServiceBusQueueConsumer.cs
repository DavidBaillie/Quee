using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quee.Interfaces;
using Quee.Messages;
using System.Text;

namespace Quee.AzureServiceBus;

internal class AzureServiceBusQueueConsumer<TMessage>
    : IHostedService, IDisposable
    where TMessage : class
{
    private readonly string connectionString;
    private readonly string queueName;
    private readonly AzureServiceBusConsumerOptions options;
    private readonly ILogger<AzureServiceBusQueueConsumer<TMessage>> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IQueueEventTrackingService? trackingService;

    private readonly ServiceBusClient serviceBusClient;
    private readonly ServiceBusProcessor serviceBusProcessor;
    private readonly ServiceBusSender serviceBusSender;

    private CancellationTokenSource? cancellationTokenSource;

    /// <summary>
    /// Build a single connection to the Service Bus to handle processing all messages for the provided queue
    /// </summary>
    /// <param name="connectionString">Connection string for the Azure Service Bus</param>
    /// <param name="queueName">Queue to work with in the service bus</param>
    public AzureServiceBusQueueConsumer(
        string connectionString,
        string queueName,
        IServiceProvider serviceProvider)
    {
        this.connectionString = connectionString;
        this.queueName = queueName;

        logger = serviceProvider.GetRequiredService<ILogger<AzureServiceBusQueueConsumer<TMessage>>>();
        serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        trackingService = serviceProvider.GetService<IQueueEventTrackingService>();
        options = serviceProvider.GetServices<AzureServiceBusConsumerOptions>()
            .FirstOrDefault(
                x => x.TargetQueue == queueName,
                defaultValue: new AzureServiceBusConsumerOptions()
            );

        // Build the client for connecting to the service bus
        serviceBusClient = new ServiceBusClient(connectionString, new ServiceBusClientOptions()
        {
            RetryOptions = new()
            {
                Delay = TimeSpan.FromSeconds(1),
                Mode = ServiceBusRetryMode.Exponential,
                MaxDelay = TimeSpan.FromSeconds(30),
                MaxRetries = 5
            }
        });

        // Build the processor with settings for controlling how the processing works
        serviceBusProcessor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions()
        {
            MaxConcurrentCalls = options.ConcurrencyLimit,
            PrefetchCount = options.PrefetchLimit
        });
        serviceBusSender = serviceBusClient.CreateSender(queueName);
    }

    /// <inheritdoc />
    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Azure Service Bus Consumer for {QueueName}", queueName);

        try
        {
            // Bind the chain of token to the source hosted service
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // If the queue needs to be checked and the queue doesn't exist (we can't create it when missing), log the message and kill the startup
            if (!await AzureServiceBusQueueManager.TryCreateQueueIfMissingAsync(connectionString, queueName, cancellationToken))
            {
                logger.LogError("Azure Service Bus Consumer for {QueueName} has failed to start because the queue does not exist and cannot be created.",
                    queueName);
                return;
            }

            // Start listening to events and run the queue consumption
            serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
            serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;
            await serviceBusProcessor.StartProcessingAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Service Bus Consumer for {QueueName} has failed to start because of an exception that was encountered during start.",
                    queueName);
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(
        CancellationToken cancellationToken)
    {
        cancellationTokenSource?.Cancel();
        await serviceBusProcessor.StopProcessingAsync(cancellationToken);
    }

    /// <summary>
    /// Processes a single message from the queue
    /// </summary>
    /// <param name="args">Message arguments</param>
    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        // Read the message back into the correct model
        var message = JsonConvert.DeserializeObject<AzureServiceBusMessage<TMessage>>(
            Encoding.UTF8.GetString(args.Message.Body.ToArray()));

        // Failed to cast message into correct type based on provided generic
        // Deadletter the message so it can be looked at later because this consumer can never process it
        if (message is null || message.Payload is null)
        {
            await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationTokenSource!.Token);
            logger.LogError("Azure Service Bus Consumer for {QueueName} has failed consume message {MessageId} because it could not be deserialized. See deadletter queue for message.",
                    queueName, args.Message.MessageId);
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<TMessage>>();

        try
        {
            // Run the developer implementation of the consumption
            // If an exception is encountered, enter retry policy and fault handler
            await consumer.ConsumeAsync(new Message<TMessage>()
            {
                Payload = message.Payload,
            }, cancellationTokenSource!.Token);
        }
        catch (Exception ex)
        {
            await HandleConsumerInvocationFailureAsync(consumer, ex, args, message, cancellationTokenSource!.Token);
            return;
        }

        // No exception encountered, complete the message as being correctly consumed
        trackingService?.RecordReceivedMessage(queueName, message.Payload);
        await args.CompleteMessageAsync(args.Message);
    }

    /// <summary>
    /// Handles resolving what to do when the consumer failed to consume the provided <see cref="ProcessMessageEventArgs"/>
    /// </summary>
    /// <param name="exception">Exception encountered</param>
    /// <param name="args">Source message event arguments</param>
    private async Task HandleConsumerInvocationFailureAsync(
        IConsumer<TMessage> consumer,
        Exception exception,
        ProcessMessageEventArgs args,
        AzureServiceBusMessage<TMessage> message,
        CancellationToken cancellationToken)
    {
        // Add the exception to the message
        message.RetryExceptions.Add($"{exception.GetType().Name} - {exception.Message}");

        // If there's no more retries, we go into fault
        if (message.RetryDelays is null || message.RetryNumber >= message.RetryDelays.Length)
        {
            try
            {
                // Save the message before processing the fault to make sure no data is lost
                trackingService?.RecordFaultedMessage(queueName, message.Payload);

                await consumer.ConsumeFaultAsync(new FaultMessage<TMessage>()
                {
                    Payload = message.Payload,
                    Exceptions = message.RetryExceptions
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                // If the fault dies we log it, dev shouldn't let this happen generally
                logger.LogError(ex, "Azure Service Bus Consumer for {QueueName} invoked the fault consumer an encountered an exception.",
                    queueName);
            }
            finally
            {
                // Always complete the message when we enter the fault logic
                await args.CompleteMessageAsync(args.Message, cancellationToken);
            }

            return;
        }

        // Retry is allowed, queue it for later consumption
        try
        {
            // Message is going to be re-qeued into the bus so to handled after the current retry timespan
            // Increment the attempt counter for the next time this is consumed
            var body = Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new AzureServiceBusMessage<TMessage>()
                    {
                        Payload = message.Payload,
                        RetryDelays = message.RetryDelays,
                        RetryExceptions = message.RetryExceptions,
                        RetryNumber = message.RetryNumber + 1
                    }));

            // Recreate the message and queue it to be processed after the developer provided span of time
            var busMessage = new ServiceBusMessage(body);
            busMessage.ScheduledEnqueueTime = DateTime.UtcNow + message.RetryDelays[message.RetryNumber];

            await serviceBusSender.SendMessageAsync(busMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Service Bus Consumer for {QueueName} processed a faulted message and is retrying for attempt {AttemptNumber}. An exception was encountered" +
                "and has prevented the message from being queued for a later attempt.",
                queueName, message.RetryNumber + 1);
        }
    }

    /// <summary>
    /// Called when the <see cref="ProcessMessageAsync(ProcessMessageEventArgs)"/> method is unable to complete
    /// Generally speaking this method should never run as the error is handled in the Process method. If it does happen I need to come back
    /// and make some fixes. Logging the event for tracking.
    /// </summary>
    /// <param name="args">Error event arguments</param>
    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Azure Service Bus Consumer for {QueueName} received an error event, message {MessageId} cannot be processed!",
                    queueName, args.Identifier);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Task.Run(async () =>
        {
            await serviceBusProcessor.DisposeAsync();
            await serviceBusSender.DisposeAsync();
            await serviceBusClient.DisposeAsync();
        }).Wait();

        cancellationTokenSource?.Dispose();
    }
}
