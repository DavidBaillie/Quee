using Microsoft.Extensions.Logging;
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
    private readonly ILogger<QueueEventTrackingService> logger;

    /// <summary>
    /// Constructor to define control for how much data is stored in the system.
    /// </summary>
    /// <param name="maximumMessagesPerQueue">Maximum messages allowed per queue</param>
    public QueueEventTrackingService(int maximumMessagesPerQueue, ILogger<QueueEventTrackingService> logger)
    {
        if (maximumMessagesPerQueue < 1)
            throw new ArgumentException("A maximum message count greater than 0 is required for the tracking service to function!");

        this.maximumMessagesPerQueue = maximumMessagesPerQueue;
        this.logger = logger;
    }

    /// <inheritdoc />
    public void RecordSentMessage<T>(string queueName, T message)
        where T : class
    {
        // Make sure the dictionary element exists
        if (!sentMessages.ContainsKey(queueName))
            sentMessages.TryAdd(queueName, []);

        // Save the message with a timestamp
        sentMessages[queueName].Enqueue(new EnqueueEvent(message, DateTime.UtcNow));
        logger.LogInformation("Adding message to sent queue {QueueName}: {Count}", queueName, sentMessages[queueName].Count);

        // Check for too many messages
        if (sentMessages[queueName].Count > maximumMessagesPerQueue)
            sentMessages[queueName].Dequeue();
    }

    /// <inheritdoc />
    public void RecordReceivedMessage<T>(string queueName, T message)
        where T : class
    {
        // Make sure the dictionary element exists
        if (!receivedMessages.ContainsKey(queueName))
            receivedMessages.TryAdd(queueName, []);

        // Save the message with a timestamp
        receivedMessages[queueName].Enqueue(new EnqueueEvent(message, DateTime.UtcNow));
        logger.LogInformation("Adding message to received queue {QueueName}: {Count}", queueName, receivedMessages[queueName].Count);

        // Check for too many messages
        if (receivedMessages[queueName].Count > maximumMessagesPerQueue)
            receivedMessages[queueName].Dequeue();
    }

    /// <inheritdoc />
    public void RecordFaultedMessage<T>(string queueName, T message)
        where T : class
    {
        // Make sure the dictionary element exists
        if (!faultedMessages.ContainsKey(queueName))
            faultedMessages.TryAdd(queueName, []);

        // Save the message with a timestamp
        faultedMessages[queueName].Enqueue(new EnqueueEvent(message, DateTime.UtcNow));
        logger.LogInformation("Adding message to fault queue {QueueName}: {Count}", queueName, faultedMessages[queueName].Count);

        // Check for too many messages
        if (faultedMessages[queueName].Count > maximumMessagesPerQueue)
            faultedMessages[queueName].Dequeue();
    }

    /// <inheritdoc />
    public bool TryGetSentMessage<T>(string queueName, [NotNullWhen(true)] out T? value, Predicate<T> searchExpression)
        where T : class
    {
        value = null;

        // No queue, no message
        if (!sentMessages.ContainsKey(queueName))
            return false;

        // Check all the elements of the queue against them being of T and matching the desired search expression
        foreach (var queuedMessage in sentMessages[queueName])
        {
            if (queuedMessage.Message is T resultMessage && searchExpression.Invoke(resultMessage))
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
        value = null;

        // No queue, no message
        if (!receivedMessages.ContainsKey(queueName))
            return false;

        // Check all the elements of the queue against them being of T and matching the desired search expression
        foreach (var queuedMessage in receivedMessages[queueName])
        {
            if (queuedMessage.Message is T resultMessage && searchExpression.Invoke(resultMessage))
            {
                value = resultMessage;
                return true;
            }
        }

        return false;
    }
}
