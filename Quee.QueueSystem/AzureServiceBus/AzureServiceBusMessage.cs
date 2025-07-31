namespace Quee.AzureServiceBus;

internal class AzureServiceBusMessage<T> where T : class
{
    public required T Payload { get; set; }
}
