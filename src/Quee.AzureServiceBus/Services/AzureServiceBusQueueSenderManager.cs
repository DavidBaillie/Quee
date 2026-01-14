using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using Quee.AzureServiceBus.Interfaces;
using System.Collections.Concurrent;

namespace Quee.AzureServiceBus.Services;

internal sealed class AzureServiceBusQueueSenderManager(IServiceProvider serviceProvider)
    : IAsyncDisposable, IAzureServiceBusQueueSenderManager
{
    private readonly ServiceBusClient serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();
    private readonly ServiceBusAdministrationClient? adminClient = serviceProvider.GetService<ServiceBusAdministrationClient>();

    private readonly ConcurrentDictionary<string, ServiceBusSender> queueSenders = [];

    public ServiceBusSender CreateSender(string queueName)
    {
        if (adminClient is not null)
        {
            if (!AzureServiceBusQueueManager.TryCreateQueueIfMissingAsync(adminClient, queueName, CancellationToken.None).Result)
                throw new TransmissionFailureException($"Azure Service Bus Sender for queue {queueName} cannot send messages because " +
                    $"it doesn't exist and the application doesn't have permission to create it.");
        }

        return queueSenders.GetOrAdd(queueName, serviceBusClient.CreateSender);
    }


    public async ValueTask DisposeAsync()
    {
        if (!queueSenders.IsEmpty)
        {
            var disposeTasks = queueSenders.Values.Select(sender => sender.DisposeAsync());
            foreach (var disposeTask in disposeTasks)
            {
                await disposeTask.ConfigureAwait(false);
            }
            queueSenders.Clear();
        }

        GC.SuppressFinalize(this);
    }
}
