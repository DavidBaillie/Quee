using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Quee.Interfaces;
using System.Text;

/// <summary>
/// Handles sending a message of <typeparamref name="TMessage"/> from the local system into an Azure Service Bus queue.
/// Contents of the message are serialized and then sent to the queue for later consumption.
/// </summary>
/// <typeparam name="TMessage">Message to be sent into the queue</typeparam>
public class AzureServiceBusQueueSender<TMessage>
    : IDisposable, IQueueSender<TMessage>
    where TMessage : class
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly ServiceBusSender serviceBusSender;

    /// <summary>
    /// Construct a connection to the Service Bus via the provided connection string and for the given queue
    /// </summary>
    /// <param name="connectionString">Connection string to the Service Bus</param>
    /// <param name="queueName">Name of the queue ti submit to</param>
    public AzureServiceBusQueueSender(string connectionString, string queueName)
    {
        serviceBusClient = new ServiceBusClient(connectionString);
        serviceBusSender = serviceBusClient.CreateSender(queueName);
    }

    /// <summary>
    /// Sends a message to a queue with a matching name as provided in construction
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendMessageAsync(TMessage message, CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(message));

        await serviceBusSender.SendMessageAsync(new ServiceBusMessage(body), cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        serviceBusSender.DisposeAsync().GetAwaiter().GetResult();
        serviceBusClient.DisposeAsync().GetAwaiter().GetResult();
    }
}