#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

internal class InMemoryMessage<T>
    where T : class
{
    /// <summary>
    /// UUID for tracking messages between invocations
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Queue that the message is intended for
    /// </summary>
    public required string TargetQueue { get; set; }

    /// <summary>
    /// The consumer will not process this message until DateTime.UtcNow is greater than this time.
    /// </summary>
    public required DateTime ProcessNotBefore { get; set; }

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
    public List<Exception> RetryExceptions { get; set; } = [];

    /// <summary>
    /// Message being sent in the queue
    /// </summary>
    public required T Payload { get; set; }
}
