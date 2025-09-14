using Quee.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Quee.Services;

/// <summary>
/// Responsible for tracking messages sent and received from the queue for inspection of flow. This will hamper the performance
/// of any application and is only intended to be used in debugging or testing environents.
/// </summary>
internal sealed class QueueEventTrackingService
    : IQueueEventTrackingService
{
    /// <summary>
    /// Wrapper for a message sent/recieved by the system
    /// </summary>
    /// <param name="Message">Message being tracked</param>
    /// <param name="EnqueueTime">When the message was recorded in the state tracker</param>
    private record EnqueueEvent(object Message, DateTime EnqueueTime);

    private readonly ConcurrentDictionary<string, Queue<EnqueueEvent>> sentMessages = new();
    private readonly ConcurrentDictionary<string, Queue<EnqueueEvent>> receivedMessages = new();
    private readonly ConcurrentDictionary<string, Queue<EnqueueEvent>> faultedMessages = new();

    private readonly int maximumMessagesPerQueue;

    /// <summary>
    /// Constructor to define control for how much data is stored in the system.
    /// </summary>
    /// <param name="maximumMessagesPerQueue">Maximum messages allowed per queue</param>
    public QueueEventTrackingService(int maximumMessagesPerQueue)
    {
        if (maximumMessagesPerQueue < 1)
            throw new ArgumentException("A maximum message count greater than 0 is required for the tracking service to function!");

        this.maximumMessagesPerQueue = maximumMessagesPerQueue;
    }

    /// <inheritdoc />
    public void RecordSentMessage<T>(string queueName, T message)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // Make sure the dictionary element exists
        var queue = sentMessages.GetOrAdd(queueName, (_) => new Queue<EnqueueEvent>());

        // Save the message with a timestamp
        queue.Enqueue(new EnqueueEvent(message, DateTime.UtcNow));

        // Check for too many messages
        if (queue.Count > maximumMessagesPerQueue)
            queue.Dequeue();
    }

    /// <inheritdoc />
    public void RecordReceivedMessage<T>(string queueName, T message)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // Make sure the dictionary element exists
        var queue = receivedMessages.GetOrAdd(queueName, (_) => new Queue<EnqueueEvent>());

        // Save the message with a timestamp
        queue.Enqueue(new EnqueueEvent(message, DateTime.UtcNow));

        // Check for too many messages
        if (queue.Count > maximumMessagesPerQueue)
            queue.Dequeue();
    }

    /// <inheritdoc />
    public void RecordFaultedMessage<T>(string queueName, T message)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // Make sure the dictionary element exists
        var queue = faultedMessages.GetOrAdd(queueName, (_) => new Queue<EnqueueEvent>());

        // Save the message with a timestamp
        queue.Enqueue(new EnqueueEvent(message, DateTime.UtcNow));

        // Check for too many messages
        if (queue.Count > maximumMessagesPerQueue)
            queue.Dequeue();
    }

    /// <inheritdoc />
    public bool TryGetSentMessage<T>(string queueName, [NotNullWhen(true)] out T? value, Predicate<T> searchExpression)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentNullException.ThrowIfNull(searchExpression, nameof(searchExpression));

        value = null;

        // No queue, no message
        if (!sentMessages.TryGetValue(queueName, out var queue))
            return false;

        // Check all the elements of the queue against them being of T and matching the desired search expression
        foreach (var queuedMessage in queue)
        {
            if (queuedMessage is null)
                throw new NullReferenceException($"Queue Event Tracking service encountered a null record in the sent messages for the {queueName} queue. " +
                    $"This should be impossible. Please submit a bug report.");

            if (queuedMessage.Message is T resultMessage && resultMessage is not null && searchExpression.Invoke(resultMessage))
            {
                value = resultMessage;
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public bool TryGetReceivedMessage<T>(string queueName, [NotNullWhen(true)] out T? value, Predicate<T> searchExpression)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentNullException.ThrowIfNull(searchExpression, nameof(searchExpression));

        value = null;

        // No queue, no message
        if (!receivedMessages.TryGetValue(queueName, out var queue))
            return false;

        // Check all the elements of the queue against them being of T and matching the desired search expression
        foreach (var queuedMessage in queue)
        {
            if (queuedMessage is null)
                throw new NullReferenceException($"Queue Event Tracking service encountered a null record in the received messages for the {queueName} queue. " +
                    $"This should be impossible. Please submit a bug report.");

            if (queuedMessage.Message is T resultMessage && resultMessage is not null && searchExpression.Invoke(resultMessage))
            {
                value = resultMessage;
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public bool TryGetFaultedMessage<T>(string queueName, [NotNullWhen(true)] out T? value, Predicate<T> searchExpression)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentNullException.ThrowIfNull(searchExpression, nameof(searchExpression));

        value = null;

        // No queue, no message
        if (!faultedMessages.TryGetValue(queueName, out var queue))
            return false;

        // Check all the elements of the queue against them being of T and matching the desired search expression
        foreach (var queuedMessage in queue)
        {
            if (queuedMessage is null)
                throw new NullReferenceException($"Queue Event Tracking service encountered a null record in the faulted messages for the {queueName} queue. " +
                    $"This should be impossible. Please submit a bug report.");

            if (queuedMessage.Message is T resultMessage && resultMessage is not null && searchExpression.Invoke(resultMessage))
            {
                value = resultMessage;
                return true;
            }
        }

        return false;
    }
}
