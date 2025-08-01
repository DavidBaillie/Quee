namespace Quee.Interfaces;

public interface IFault<T>
{
    T Payload { get; }
    IEnumerable<string> Exceptions { get; }
}
