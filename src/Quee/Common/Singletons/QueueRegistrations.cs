#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Singleton handles tracking all senders and receivers registered by the current runtime during program startup.
/// </summary>
public static class QueueRegistrations
{
    /// <summary>
    /// Names of all the queues that have senders registered in the runtime
    /// </summary>
    public static HashSet<string> Senders = [];

    /// <summary>
    /// Names of all the queues that have consumers registered in the runtime
    /// </summary>
    public static HashSet<string> Consumers = [];
}
