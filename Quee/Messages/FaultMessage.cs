namespace Quee.Messages;

public class FaultMessage<T> where T : class
{
    public virtual required T Payload { get; set; }
    public virtual IEnumerable<string> Exceptions { get; set; } = [];
}
