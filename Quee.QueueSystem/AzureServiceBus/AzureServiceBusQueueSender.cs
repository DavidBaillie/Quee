using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Quee.Exceptions;
using Quee.Interfaces;
using System.Text;

namespace Quee.AzureServiceBus;

/// <summary>
/// Handles sending a message of <typeparamref name="TMessage"/> from the local system into an Azure Service Bus queue.
/// Contents of the message are serialized and then sent to the queue for later consumption.
/// </summary>
/// <typeparam name="TMessage">Message to be sent into the queue</typeparam>
public class AzureServiceBusQueueSender<TMessage>
    : IDisposable, IQueueSender<TMessage>
    where TMessage : class
{
    private readonly string connectionString;
    private readonly string queueName;

    private readonly ServiceBusClient serviceBusClient;
    private readonly ServiceBusSender serviceBusSender;
    private readonly TimeSpan[] retrySpans;

    private bool hasCheckedQueueExists = false;
    private bool queueExists = false;

    /// <summary>
    /// Construct a connection to the Service Bus via the provided connection string and for the given queue
    /// </summary>
    /// <param name="connectionString">Connection string to the Service Bus</param>
    /// <param name="queueName">Name of the queue to submit to</param>
    /// <param name="retrySpans">Timespans between each allowed retry</param>
    public AzureServiceBusQueueSender(
        string connectionString,
        string queueName,
        params TimeSpan[] retrySpans)
    {
        this.connectionString = connectionString;
        this.queueName = queueName;
        this.retrySpans = retrySpans;

        serviceBusClient = new ServiceBusClient(connectionString);
        serviceBusSender = serviceBusClient.CreateSender(queueName);
    }

    /// <summary>
    /// Sends a message to a queue with a matching name as provided in construction
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="cancellationToken">Process token</param>
    public async Task SendMessageAsync(
        TMessage message,
        CancellationToken cancellationToken)
    {
        // Check that the queue exists only once
        if (!hasCheckedQueueExists)
        {
            hasCheckedQueueExists = true;
            queueExists = await AzureServiceBusQueueManager.TryCreateQueueIfMissingAsync(connectionString, queueName, cancellationToken);
        }

        // If the queue doesn't exist, we can't send a message to it
        if (!queueExists)
        {
            throw new TransmissionFailureException($"Azure Service Bus Sender for {typeof(TMessage).GetType().Name} cannot send messages to queue {queueName} because " +
                $"it doesn't exist and the application doesn't have permission to create it.");
        }

        // Wrap the user payload in a retry wrapper, serialize to a string, and then encode for transmission to service bus
        var body = Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(
                new AzureServiceBusMessage<TMessage>()
                {
                    Payload = message,
                    RetryDelays = retrySpans
                }));

        var busMessage = new ServiceBusMessage(body);
        await serviceBusSender.SendMessageAsync(busMessage, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        serviceBusSender.DisposeAsync().GetAwaiter().GetResult();
        serviceBusClient.DisposeAsync().GetAwaiter().GetResult();
    }
}