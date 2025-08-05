using Quee.Interfaces;

namespace Quee.Services;

internal class QueueMonitor(IQueueEventTrackingService trackingService) : IQueueMonitor
{
    public async Task<TMessage?> WaitForMessageToSend<TMessage>(
        string queueName,
        TimeSpan maximumWaitTime,
        CancellationToken cancellationToken,
        Predicate<TMessage> searchExpression,
        int pollDelay = 100)
        where TMessage : class
    {
        if (maximumWaitTime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maximumWaitTime));

        var expireTime = DateTime.UtcNow + maximumWaitTime;
        while (DateTime.UtcNow < expireTime && !cancellationToken.IsCancellationRequested)
        {
            if (trackingService.TryGetSentMessage(queueName, out TMessage? value, searchExpression))
                return value;

            await Task.Delay(pollDelay);
        }

        return null;
    }

    public async Task<TMessage?> WaitForMessageToReceive<TMessage>(
        string queueName,
        TimeSpan maximumWaitTime,
        CancellationToken cancellationToken,
        Predicate<TMessage> searchExpression,
        int pollDelay = 100)
        where TMessage : class
    {
        if (maximumWaitTime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maximumWaitTime));

        var expireTime = DateTime.UtcNow + maximumWaitTime;
        while (DateTime.UtcNow < expireTime && !cancellationToken.IsCancellationRequested)
        {
            if (trackingService.TryGetReceivedMessage(queueName, out TMessage? value, searchExpression))
                return value;

            await Task.Delay(pollDelay);
        }

        return null;
    }

    public async Task<TMessage?> WaitForMessageToFault<TMessage>(
        string queueName,
        TimeSpan maximumWaitTime,
        CancellationToken cancellationToken,
        Predicate<TMessage> searchExpression,
        int pollDelay = 100)
        where TMessage : class
    {
        if (maximumWaitTime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maximumWaitTime));

        var expireTime = DateTime.UtcNow + maximumWaitTime;
        while (DateTime.UtcNow < expireTime && !cancellationToken.IsCancellationRequested)
        {
            if (trackingService.TryGetFaultedMessage(queueName, out TMessage? value, searchExpression))
                return value;

            await Task.Delay(pollDelay);
        }

        return null;
    }
}
