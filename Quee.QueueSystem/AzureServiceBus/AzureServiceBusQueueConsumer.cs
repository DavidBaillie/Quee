using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quee.Interfaces;
using System.Text;

namespace Quee.AzureServiceBus;

public class AzureServiceBusQueueConsumer<TMessage>
    : IHostedService, IDisposable
    where TMessage : class
{
    private readonly ILogger<AzureServiceBusQueueConsumer<TMessage>> logger;
    private readonly string connectionString;
    private readonly string queueName;
    private readonly IConsumer<TMessage> consumer;
    private readonly bool createQueueWhenMissing;
    private readonly ServiceBusClient serviceBusClient;
    private readonly ServiceBusProcessor serviceBusProcessor;

    private CancellationTokenSource? cancellationTokenSource;

    /// <summary>
    /// Build a single connection to the Service Bus to handle processing all messages for the provided queue
    /// </summary>
    /// <param name="connectionString">Connection string for the Azure Service Bus</param>
    /// <param name="queueName">Queue to work with in the service bus</param>
    public AzureServiceBusQueueConsumer(
        ILogger<AzureServiceBusQueueConsumer<TMessage>> logger,
        string connectionString,
        string queueName,
        IConsumer<TMessage> consumer,
        bool createQueueWhenMissing = true)
    {
        this.logger = logger;
        this.connectionString = connectionString;
        this.queueName = queueName;
        this.consumer = consumer;
        this.createQueueWhenMissing = createQueueWhenMissing;

        // Build the client for connecting to the service bus
        serviceBusClient = new ServiceBusClient(connectionString, new ServiceBusClientOptions()
        {
            RetryOptions = new()
            {
                Delay = TimeSpan.FromSeconds(5),
                Mode = ServiceBusRetryMode.Exponential,
                MaxDelay = TimeSpan.FromSeconds(60),
                MaxRetries = 5
            }
        });
        serviceBusProcessor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions());
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Azure Service Bus Consumer for {MessageType}", typeof(TMessage).GetType().Name);

        try
        {
            // Bind the chain of token to the source hosted service
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // If the queue needs to be checked and the queue doesn't exist (we can't create it when missing), log the message and kill the startup 
            if (createQueueWhenMissing && !await TryCreateQueueIfMissingAsync(cancellationToken))
            {
                logger.LogError("Azure Service Bus Consumer for {MessageType} has failed to start because the queue could not be validated!",
                    typeof(TMessage).GetType().Name);
                return;
            }

            // Start listening to events and run the queue consumption
            serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
            serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;
            await serviceBusProcessor.StartProcessingAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Service Bus Consumer for {MessageType} has failed to start because of an exception that was encountered during start.",
                    typeof(TMessage).GetType().Name);
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource?.Cancel();
        await serviceBusProcessor.StopProcessingAsync(cancellationToken);
    }

    /// <summary>
    /// Checks to see if the queue for this consumer exists and attempts to create it if missing.
    /// </summary>
    /// <param name="cancellationToken">Process token</param>
    /// <returns>If the qeue exists at the time this method return</returns>
    private async Task<bool> TryCreateQueueIfMissingAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = new ServiceBusAdministrationClient(connectionString);

            if (!await client.QueueExistsAsync(queueName))
            {
                await client.CreateQueueAsync(queueName, cancellationToken);
                logger.LogInformation("Azure Service Bus Consumer for {MessageType} discovered queue {QueueName} was missing. Queue has been added to the service bus.",
                    typeof(TMessage).GetType().Name, queueName);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Service Bus Consumer for {MessageType} attempted to create queue {QueueName} but encountered an exception",
                typeof(TMessage).GetType().Name, queueName);
            return false;
        }
    }

    /// <summary>
    /// Processes a single message from the queue
    /// </summary>
    /// <param name="args">Message arguments</param>
    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var message = JsonConvert.DeserializeObject<TMessage>(
            Encoding.UTF8.GetString(args.Message.Body.ToArray()));
        Console.WriteLine($"Received message: {message}");

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error occurred: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        cancellationTokenSource?.Dispose();
        serviceBusProcessor.DisposeAsync().GetAwaiter().GetResult();
        serviceBusClient.DisposeAsync().GetAwaiter().GetResult();
    }
}
