using System.Diagnostics.CodeAnalysis;

namespace Quee.Interfaces;

internal interface IMemoryQueue
{
    bool TryReadMessage<T>(string queue, [NotNullWhen(true)] out T? message, bool discardMismatchedMessages = false) where T : class;
    void WriteMessage<T>(string queue, T message, TimeSpan? scheduledDelay = null) where T : class;
}