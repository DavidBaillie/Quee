namespace Quee.Interfaces;

/// <summary>
/// Monitors the state of the queue and allows waiting for messages process through the possible states of the queue
/// </summary>
public interface IQueueMonitor
{
    /// <summary>
    /// Awaits the queue to process the a message that caused a fault
    /// </summary>
    /// <typeparam name="TMessage">Message to poll for</typeparam>
    /// <param name="queueName">Name of queue message faulted in</param>
    /// <param name="maximumWaitTime">Max time to wait for message to process</param>
    /// <param name="searchExpression">Expression to identify message</param>
    /// <param name="pollDelay">Check rate against the tracking system</param>
    /// <returns>The message when found, null when not</returns>
    Task<TMessage?> WaitForMessageToFault<TMessage>(
        string queueName,
        TimeSpan maximumWaitTime,
        CancellationToken cancellationToken,
        Predicate<TMessage> searchExpression,
        int pollDelay = 100)
        where TMessage : class;

    /// <summary>
    /// Awaits the queue to process a message as being correctly received
    /// </summary>
    /// <typeparam name="TMessage">Message to poll for</typeparam>
    /// <param name="queueName">Name of queue message was received in</param>
    /// <param name="maximumWaitTime">Max time to wait for message to process</param>
    /// <param name="searchExpression">Expression to identify message</param>
    /// <param name="pollDelay">Check rate against the tracking system</param>
    /// <returns>The message when found, null when not</returns>
    Task<TMessage?> WaitForMessageToReceive<TMessage>(
        string queueName,
        TimeSpan maximumWaitTime,
        CancellationToken cancellationToken,
        Predicate<TMessage> searchExpression,
        int pollDelay = 100)
        where TMessage : class;

    /// <summary>
    /// Awaits the queue to process a message being sent into the queue
    /// </summary>
    /// <typeparam name="TMessage">Message to poll for</typeparam>
    /// <param name="queueName">Name of queue message was sent into</param>
    /// <param name="maximumWaitTime">Max time to wait for message to process</param>
    /// <param name="searchExpression">Expression to identify message</param>
    /// <param name="pollDelay">Check rate against the tracking system</param>
    /// <returns>The message when found, null when not</returns>
    Task<TMessage?> WaitForMessageToSend<TMessage>(
        string queueName,
        TimeSpan maximumWaitTime,
        CancellationToken cancellationToken,
        Predicate<TMessage> searchExpression,
        int pollDelay = 100)
        where TMessage : class;
}