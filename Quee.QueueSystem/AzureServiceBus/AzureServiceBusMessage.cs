using Quee.Abstracts;

namespace Quee.AzureServiceBus;

internal class AzureServiceBusMessage<T> : MessageBase<T>
    where T : class;
