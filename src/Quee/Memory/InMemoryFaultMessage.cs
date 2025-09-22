using Quee.Messages;

namespace Quee.Memory;

internal class InMemoryFaultMessage<T>
    : FaultMessage<T>
    where T : class
{
    public IEnumerable<Exception> SourceExceptions { get; set; } = [];
    public override IEnumerable<string> Exceptions
    {
        get => SourceExceptions.Select(x => $"{x.GetType().Name} - {x.Message}");
    }
}
