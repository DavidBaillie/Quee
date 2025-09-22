namespace Quee.AzureServiceBus;

/// <summary>
/// Defines the message wrapper as sent into the Azure Service Bus when messages are passed
/// from the local runtime into the external queue.
/// </summary>
/// <typeparam name="T">Type of message to wrap</typeparam>
internal class AzureServiceBusMessage<T>
    where T : class
{
    /// <summary>
    /// Attempt number in the retry series
    /// </summary>
    public int RetryNumber { get; set; }

    /// <summary>
    /// Array of time spans for how long to wait between each attempt
    /// </summary>
    public TimeSpan[] RetryDelays { get; set; } = [];

    /// <summary>
    /// Exceptions encountered while attempting to invoke the consumer across retries
    /// </summary>
    public List<string> RetryExceptions { get; set; } = [];

    /// <summary>
    /// Message being sent in the queue
    /// </summary>
    public required T Payload { get; set; }
}
