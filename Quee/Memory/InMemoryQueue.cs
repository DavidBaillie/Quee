using Quee.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Quee.Memory;

/// <summary>
/// Represents the queue itself for the in-memory provider to allow for messages to be received and sent to consumers
/// on the local machine only.
/// </summary>
internal sealed class InMemoryQueue
    : IMemoryQueue
{
    private readonly object Lock = new();

    /// <summary>
    /// Store of all queued messages in the system
    /// </summary>
    private readonly Dictionary<string, PriorityQueue<object, DateTime>> queuedMessages = new();

    /// <summary>
    /// Writes a message to the in-memory queue
    /// </summary>
    /// <param name="queueName">Name of the queue to save the message to</param>
    /// <param name="message">Message to send into the queue for later consumption</param>
    public void WriteMessage<T>(string queueName, T message, TimeSpan? scheduledDelay = null)
        where T : class
    {
        // Lock all read/write operations to be sequential
        lock (Lock)
        {
            if (queuedMessages.TryGetValue(queueName, out var existingQueue))
            {
                existingQueue.Enqueue(message, scheduledDelay.HasValue ? DateTime.UtcNow + scheduledDelay.Value : DateTime.MinValue);
            }
            else
            {
                var createdQueue = new PriorityQueue<object, DateTime>();
                createdQueue.Enqueue(message, scheduledDelay.HasValue ? DateTime.UtcNow + scheduledDelay.Value : DateTime.MinValue);
                queuedMessages.Add(queueName, createdQueue);
            }
        }
    }

    /// <summary>
    /// Attempts to read a message from the queue as <typeparamref name="T"/>. Will return false when the queue has never been created,
    /// the queue has no messages in it, or when the message found in the queue is not of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Type of message to read from the queue</typeparam>
    /// <param name="queueName">Name of the queue to read from</param>
    /// <param name="message">The matching message that was found inside the queue</param>
    /// <param name="discardMismatchedMessages">
    /// In the event that a message was found but not of type <typeparamref name="T"/>, should the message be discarded from the queue or left in the queue
    /// for the next read attempt from another consumer?
    /// </param>
    /// <returns>If a message was found and could be read</returns>
    public bool TryReadMessage<T>(string queueName, [NotNullWhen(true)] out T? message, bool discardMismatchedMessages = false)
        where T : class
    {
        // Lock all read/write operations to be sequential
        lock (Lock)
        {
            message = null;

            if (!queuedMessages.TryGetValue(queueName, out var existingQueue))
                return false;

            // Queue empty, do nothing
            if (existingQueue.Count < 1)
                return false;

            // Queue has nothing in it, or the message shouldn't be delivered yet, or the element in the qeue isn't of type T
            if (!existingQueue.TryPeek(out var sample, out DateTime scheduleDateTime) ||
                scheduleDateTime > DateTime.UtcNow ||
                sample is not T castedMessage)
                return false;

            // Element we peeked is what was desired, remove it from the queue
            existingQueue.Dequeue();

            message = castedMessage;
            return true;
        }
    }
}
