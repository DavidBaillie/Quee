using System.Threading.Channels;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

internal class InMemoryQueueSender<TMessage>(
    string queueName,
    QueueRetryOptions retryOptions,
    TimeSpan[] retries,
    Channel<InMemoryMessage<TMessage>> channel,
    IQueueEventTrackingService? trackingService)
    : IQueueSender<TMessage>
    where TMessage : class
{
    /// <inheritdoc />
    public async Task SendMessageAsync(TMessage message, CancellationToken cancellationToken, TimeSpan? queueDelay = null)
    {
        // Write the message into the queue 
        await channel.Writer.WriteAsync(new InMemoryMessage<TMessage>()
        {
            TargetQueue = queueName,
            ProcessNotBefore = DateTime.UtcNow + (queueDelay ?? TimeSpan.Zero),
            Payload = message,
            RetryDelays = retryOptions.AllowRetries ? retries : [],
            RetryNumber = 0
        });

        // If the tracker is loaded, update the tracker that the message was sent into the queue
        trackingService?.RecordSentMessage(queueName, message);
    }
}
