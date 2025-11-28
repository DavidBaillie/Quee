#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

public interface IAzureServiceBusQueueConfigurator
    : IQueueConfigurator
{
    /// <summary>
    /// Adds a single consumer capable of taking messages from the queue and processing them in the runtime
    /// </summary>
    /// <typeparam name="TMessage">Message the consumer is responsible processing</typeparam>
    /// <typeparam name="TConsumer">Class responsible for consuming the messaeg</typeparam>
    /// <param name="queueName">Name of the queue</param>
    IAzureServiceBusQueueConfigurator AddConsumer<TMessage, TConsumer>(string queueName, ConsumerOptions options)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>;

    /// <summary>
    /// Adds a pair of sender and receiver for the message type <typeparamref name="TMessage"/> to handle sending and receiving messages in this process
    /// </summary>
    /// <typeparam name="TMessage">Message to be sent in the queue</typeparam>
    /// <typeparam name="TConsumer">Consumer implementation to handle messages</typeparam>
    /// <param name="queueName">Name of the queue to subscribe to</param>
    /// <param name="retries">Allowed retry time spans between each attempt</param>
    IAzureServiceBusQueueConfigurator AddSenderAndConsumer<TMessage, TConsumer>(string queueName, ConsumerOptions options, params TimeSpan[] retries)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>;

    /// <summary>
    /// Adds the provided <paramref name="options"/> to the registered consumer for the given <paramref name="queueName"/>
    /// </summary>
    /// <param name="queueName">Name of the queue these options will apply to</param>
    /// <param name="options">Options to apply to queue consumption</param>
    IAzureServiceBusQueueConfigurator AddQueueConsumerOptions(string queueName, ConsumerOptions options);
}
