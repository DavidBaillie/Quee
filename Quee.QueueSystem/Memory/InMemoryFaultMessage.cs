using Quee.Abstracts;

namespace Quee.Memory;

internal class InMemoryFaultMessage<TMessage>
    : FaultMessageBase<TMessage>
    where TMessage : class;
