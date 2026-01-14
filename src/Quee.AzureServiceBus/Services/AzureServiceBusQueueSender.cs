using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Quee.AzureServiceBus.Interfaces;
using Quee.AzureServiceBus.Models;
using System.Text;

namespace Quee.AzureServiceBus.Services;

/// <summary>
/// Handles sending a message of <typeparamref name="TMessage"/> from the local system into an Azure Service Bus queue.
/// Contents of the message are serialized and then sent to the queue for later consumption.
/// </summary>
/// <typeparam name="TMessage">Message to be sent into the queue</typeparam>
internal class AzureServiceBusQueueSender<TMessage>
    : IQueueSender<TMessage>
    where TMessage : class
{
    private readonly ServiceBusSender serviceBusSender;
    private readonly string queueName;
    private readonly QueueRetryOptions messageRetryOptions;
    private readonly IQueueEventTrackingService? queueTrackingService;
    private readonly TimeSpan[] messageRetryDelays;


    /// <summary>
    /// Construct a connection to the Service Bus via the provided connection string and for the given queue
    /// </summary>
    /// <param name="connectionString">Connection string to the Service Bus</param>
    /// <param name="queueName">Name of the queue to submit to</param>
    /// <param name="retrySpans">Timespans between each allowed retry</param>
    internal AzureServiceBusQueueSender(
        IAzureServiceBusQueueSenderManager queueSenderManager,
        string queueName,
        QueueRetryOptions options,
        IQueueEventTrackingService? trackingService = null,
        params TimeSpan[] retrySpans)
    {
        this.queueName = queueName;
        serviceBusSender = queueSenderManager.CreateSender(queueName);
        queueTrackingService = trackingService;
        messageRetryOptions = options;
        messageRetryDelays = retrySpans;
    }

    /// <summary>
    /// Sends a message to a queue with a matching name as provided in construction
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="cancellationToken">Process token</param>
    public async Task SendMessageAsync(
        TMessage message,
        CancellationToken cancellationToken,
        TimeSpan? initialDelay = null)
    {
        // Wrap the user payload in a retry wrapper, serialize to a string, and then encode for transmission to service bus
        var body = Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(
                new AzureServiceBusMessage<TMessage>()
                {
                    Payload = message,
                    RetryDelays = messageRetryOptions.AllowRetries ? messageRetryDelays : []
                }));

        var busMessage = new ServiceBusMessage(body);

        // Allow the user to schedule an initial delay before the message can be consumed in the queue
        if (initialDelay.HasValue)
            busMessage.ScheduledEnqueueTime = DateTime.UtcNow + initialDelay.Value;

        queueTrackingService?.RecordSentMessage(queueName, message);
        await serviceBusSender.SendMessageAsync(busMessage, cancellationToken);
    }
}