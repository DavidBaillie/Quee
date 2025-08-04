using Quee.Interfaces;

namespace Quee.Abstracts;

internal abstract class FaultMessageBase<TMessage>
: IFault<TMessage> where TMessage : class
{
    public virtual required TMessage Payload { get; set; }
    public virtual IEnumerable<string> Exceptions { get; set; } = [];
}
