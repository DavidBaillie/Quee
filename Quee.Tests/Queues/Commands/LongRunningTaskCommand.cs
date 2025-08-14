namespace Quee.Tests.Queues.Commands;

/// <summary>
/// Command to process a generic task that takes <paramref name="milisecondsToWait"/> to complete. 
/// Designed to simulate some long running task that will consume CPU cycles.
/// </summary>
/// <param name="milisecondsToWait">Miliseconds to wait before command is complete</param>
internal record LongRunningTaskCommand(int millisecondsToWait)
{
    /// <summary>
    /// Used to identify the message when searching amoung duplicates of the message in the queue
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    public int milisecondsToWait { get; set; } = millisecondsToWait;
}
