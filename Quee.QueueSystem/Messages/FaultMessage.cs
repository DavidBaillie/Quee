namespace Quee.Messages;

public sealed class FaultMessage<T> where T : class
{
    public required T Payload { get; set; }
    public IEnumerable<string> Exceptions { get; set; } = [];
}
