namespace Quee.Interfaces;

/// <summary>
/// Defines the common capabilities all queue providers support
/// </summary>
public interface IQueueConfigurator
{
    /// <summary>
    /// Adds a single sender capable of sending messages to the queue for later consumption
    /// </summary>
    /// <typeparam name="TMessage">Message the sender is responsible for transmitting</typeparam>
    /// <param name="queueName">Name of queue to send message to</param>
    IQueueConfigurator AddQueueSender<TMessage>(string queueName, params TimeSpan[] retries)
        where TMessage : class;

    /// <summary>
    /// Adds a single consumer capable of taking messages from the queue and processing them in the runtime
    /// </summary>
    /// <typeparam name="TMessage">Message the consumer is responsible processing</typeparam>
    /// <typeparam name="TConsumer">Class responsible for consuming the messaeg</typeparam>
    /// <param name="queueName">Name of the queue</param>
    IQueueConfigurator AddQueueConsumer<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>;
}
