using Azure.Messaging.ServiceBus;

namespace Quee.AzureServiceBus.Interfaces
{
    internal interface IAzureServiceBusQueueSenderManager
    {
        ServiceBusSender CreateSender(string queueName);
    }
}