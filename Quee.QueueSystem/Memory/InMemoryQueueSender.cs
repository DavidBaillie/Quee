using Quee.Interfaces;
using Quee.QueueOptions;

namespace Quee.Memory;

/// <summary>
/// Sends messages to thw in-memory queue to be processed by a consumer later
/// </summary>
/// <typeparam name="TMessage">Message to be sent to the queue</typeparam>
internal class InMemoryQueueSender<TMessage>
    : IQueueSender<TMessage>
    where TMessage : class
{
    private readonly string queueName;
    private readonly IMemoryQueue queue;
    private readonly QueueRetryOptions retryOptions;
    private readonly IQueueEventTrackingService? trackingService;
    private readonly TimeSpan[] retries;

    /// <summary>
    /// Constructs a sender for the given <paramref name="queue"/> 
    /// </summary>
    /// <param name="queueName">Name of the queue to send messages to</param>
    /// <param name="queue">Queue provider for the in-memory queue</param>
    /// <param name="retries">Allowed retries for the message</param>
    public InMemoryQueueSender(
        string queueName,
        IMemoryQueue queue,
        QueueRetryOptions retryOptions,
        IQueueEventTrackingService? trackingService,
        TimeSpan[] retries)
    {
        this.queueName = queueName;
        this.queue = queue;
        this.retryOptions = retryOptions;
        this.trackingService = trackingService;
        this.retries = retries;
    }

    /// <inheritdoc />
    public Task SendMessageAsync(TMessage message, CancellationToken cancellationToken, TimeSpan? queueDelay = null)
    {
        // Write a message to the memory queue
        queue.WriteMessage(queueName, new InMemoryMessage<TMessage>()
        {
            Payload = message,
            RetryDelays = retryOptions.AllowRetries ? retries : [],
            RetryNumber = 0
        });

        trackingService?.RecordSentMessage(queueName, message);
        return Task.CompletedTask;
    }
}
