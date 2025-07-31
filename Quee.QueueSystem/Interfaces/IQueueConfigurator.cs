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
    IQueueConfigurator AddQueueSender<TMessage>(string queueName)
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

    /// <summary>
    /// Adds a sender and receiver for the queue under a single name to handle sending messages into the queue 
    /// as well as consuming them from the queue.
    /// </summary>
    /// <typeparam name="TMessage">Message the consumer is responsible processing</typeparam>
    /// <typeparam name="TConsumer">Class responsible for consuming the messaeg</typeparam>
    /// <param name="queueName">Name of the queue</param>
    IQueueConfigurator AddQueueProcessors<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>;
}
