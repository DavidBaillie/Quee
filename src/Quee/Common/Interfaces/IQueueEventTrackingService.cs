using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Provides the ability to monitor and query all message sent and received by the local runtime. Generally an implementation for this
/// interface should only be registers in development and test builds as it adds unnecessary overhead to the runtime that will slow down
/// performance and increase memory consumption.
/// </summary>
public interface IQueueEventTrackingService
{
    /// <summary>
    /// Saves the provided message as being received and processed by a consumers fault function in the local system
    /// </summary>
    /// <typeparam name="T">Type of message being stored</typeparam>
    /// <param name="queueName">Name of queue fault happened in</param>
    /// <param name="message">Message that caused a fault to save</param>
    void RecordFaultedMessage<T>(string queueName, T message) where T : class;

    /// <summary>
    /// Saves the provided message as being received and processed by a consumer in the local system
    /// </summary>
    /// <typeparam name="T">Type of message to record</typeparam>
    /// <param name="queueName">Name of the queue the message was received from</param>
    /// <param name="message">Message to be saved</param>
    void RecordReceivedMessage<T>(string queueName, T message) where T : class;

    /// <summary>
    /// Saves the provided message as being sent by a local sender
    /// </summary>
    /// <typeparam name="T">Type of message being sent into the queue</typeparam>
    /// <param name="queueName">Name of the queue the message was sent to</param>
    /// <param name="message">Message that was sent</param>
    void RecordSentMessage<T>(string queueName, T message) where T : class;

    /// <summary>
    /// Tries to find a message of <typeparamref name="T"/> in the collection of received messages for the provided queue.
    /// </summary>
    /// <typeparam name="T">Type of message to read</typeparam>
    /// <param name="queueName">Name of queue to search</param>
    /// <param name="value">Matching message recorded in the queue</param>
    /// <param name="searchExpression">Predicate for matching the message on</param>
    /// <returns>True if a message was found, false if no matching message present</returns>
    bool TryGetReceivedMessage<T>(string queueName, [NotNullWhen(true)] out T? value, Predicate<T> searchExpression) where T : class;

    /// <summary>
    /// Tries to find a message of <typeparamref name="T"/> in the collection of sent messages for the provided queue.
    /// </summary>
    /// <typeparam name="T">Type of message to read</typeparam>
    /// <param name="queueName">Name of queue to search</param>
    /// <param name="value">Matching message recorded in the queue</param>
    /// <param name="searchExpression">Predicate for matching the message on</param>
    /// <returns>True if a message was found, false if no matching message present</returns>
    bool TryGetSentMessage<T>(string queueName, [NotNullWhen(true)] out T? value, Predicate<T> searchExpression) where T : class;

    /// <summary>
    /// Tries to find a message of <typeparamref name="T"/> which has faulted
    /// </summary>
    /// <typeparam name="T">Type of message to search for</typeparam>
    /// <param name="queueName">Name of queue fault happened in</param>
    /// <param name="value">Found message</param>
    /// <param name="searchExpression">Search predicate to identify message by</param>
    /// <returns>True if the message was found, false if no matching message present</returns>
    bool TryGetFaultedMessage<T>(string queueName, [NotNullWhen(true)] out T? value, Predicate<T> searchExpression) where T : class;
}