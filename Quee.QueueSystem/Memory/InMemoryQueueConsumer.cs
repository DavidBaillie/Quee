using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quee.Interfaces;

namespace Quee.Memory;

/// <summary>
/// Defines a background service that polls the in-memory queue and consumes messages for the matching message type <typeparamref name="TMessage"/>
/// </summary>
/// <typeparam name="TMessage">Message to consume</typeparam>
internal class InMemoryQueueConsumer<TMessage>
    : BackgroundService
    where TMessage : class
{
    private readonly string queueName;
    private readonly int secondsBetweenPolls;
    private readonly IConsumer<TMessage> consumer;
    private readonly IMemoryQueue queue;

    private readonly IQueueEventTrackingService? trackingService;
    private readonly ILogger<InMemoryQueueConsumer<TMessage>> logger;

    /// <summary>
    /// Constructs a process capable of listening to the in-memory queue and processing messages of <typeparamref name="TMessage"/>
    /// </summary>
    /// <param name="queueName">Name of the queue to observe</param>
    /// <param name="consumer">Consume to process messages with</param>
    /// <param name="millisecondsBetweenPolls">Number of milliseconds between each polling attempt</param>
    /// <param name="trackingService">Optional tracking service</param>
    public InMemoryQueueConsumer(
        string queueName,
        int millisecondsBetweenPolls,
        IConsumer<TMessage> consumer,
        IMemoryQueue queue,
        ILogger<InMemoryQueueConsumer<TMessage>> logger,
        IQueueEventTrackingService? trackingService)
    {
        this.queueName = queueName;
        this.consumer = consumer;
        this.queue = queue;
        this.logger = logger;
        this.secondsBetweenPolls = millisecondsBetweenPolls;
        this.trackingService = trackingService;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run forever while main process running
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMilliseconds(secondsBetweenPolls));
        }
    }

    /// <summary>
    /// Polls the memory queue for a message of <typeparamref name="TMessage"/>, consumes the message if one is available
    /// </summary>
    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        // No message to read, skip the event
        // If we find a incorrectly typed message, discard it (generally should never happen)
        if (!queue.TryReadMessage(queueName, out InMemoryMessage<TMessage>? message, true))
            return;

        try
        {
            // try to consume the message, catch any exception that might prevent it
            await consumer.ConsumeAsync(message.Payload, cancellationToken);
            trackingService?.RecordReceivedMessage(queueName, message.Payload);
        }
        catch (Exception ex)
        {
            // Exception raised in the consumer, move to error processing
            await HandleFailureAsync(message, ex, cancellationToken);
        }
    }

    /// <summary>
    /// Consumption of the <paramref name="message"/> encountered a failure, process the <paramref name="exception"/> into either a retry or a fault
    /// </summary>
    /// <param name="message">Message that failed</param>
    /// <param name="exception">Exception encountered during consumption</param>
    /// <param name="cancellationToken">Process token</param>
    private async Task HandleFailureAsync(InMemoryMessage<TMessage> message, Exception exception, CancellationToken cancellationToken)
    {
        List<string> structuredExceptions = [.. message.RetryExceptions, $"{exception.GetType().Name} - {exception.Message}"];

        // Ran out of retries
        if (message.RetryDelays is null || message.RetryNumber >= message.RetryDelays.Length)
        {
            try
            {
                trackingService?.RecordFaultedMessage(queueName, message.Payload);
                await consumer.ConsumeFaultAsync(new InMemoryFaultMessage<TMessage>()
                {
                    Payload = message.Payload,
                    Exceptions = structuredExceptions
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                // If the fault dies we log it, dev shouldn't let this happen generally
                logger.LogError(ex, "Azure Service Bus Consumer for {MessageType} invoked the fault consumer an encountered an exception.",
                    typeof(TMessage).GetType().Name);
            }

            return;
        }

        // Retry allowed, queue for another attempt
        queue.WriteMessage(queueName, new InMemoryMessage<TMessage>()
        {
            Payload = message.Payload,
            RetryDelays = message.RetryDelays,
            RetryNumber = message.RetryNumber + 1,
            RetryExceptions = structuredExceptions
        }, message.RetryDelays[message.RetryNumber]);
    }
}
