using Quee.Interfaces;

namespace Quee.AzureServiceBus;

internal class AzureServiceBusFaultMessage<TMessage>
    : IFault<TMessage> where TMessage : class
{
    public required TMessage Payload { get; set; }
    public IEnumerable<string> Exceptions { get; set; } = [];
}
