#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

public class QueueRetryOptions
{
    /// <summary>
    /// Defines if the queue system will allow for any retries. When this is set to false, all queues will ignore
    /// provided retry options and only ever process a message once.
    /// </summary>
    public bool AllowRetries { get; set; } = true;
}
