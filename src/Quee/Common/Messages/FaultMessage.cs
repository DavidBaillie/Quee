#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

public class FaultMessage<T> where T : class
{
    public virtual required T Payload { get; set; }
    public virtual IEnumerable<string> Exceptions { get; set; } = [];
}
