namespace Quee.Abstracts;

internal class MessageBase<T> where T : class
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
    /// Developer message being sent in the queue
    /// </summary>
    public required T Payload { get; set; }
}
