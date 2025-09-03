﻿using Quee.AzureServiceBus;

namespace Quee.Interfaces;

public interface IAzureServiceBusQueueConfigurator
    : IQueueConfigurator
{
    /// <summary>
    /// Adds a single consumer capable of taking messages from the queue and processing them in the runtime
    /// </summary>
    /// <typeparam name="TMessage">Message the consumer is responsible processing</typeparam>
    /// <typeparam name="TConsumer">Class responsible for consuming the messaeg</typeparam>
    /// <param name="queueName">Name of the queue</param>
    IAzureServiceBusQueueConfigurator AddConsumer<TMessage, TConsumer>(string queueName, AzureServiceBusConsumerOptions options)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>;

    /// <summary>
    /// Adds a pair of sender and receiver for the message type <typeparamref name="TMessage"/> to handle sending and receiving messages in this process
    /// </summary>
    /// <typeparam name="TMessage">Message to be sent in the queue</typeparam>
    /// <typeparam name="TConsumer">Consumer implementation to handle messages</typeparam>
    /// <param name="queueName">Name of the queue to subscribe to</param>
    /// <param name="retries">Allowed retry time spans between each attempt</param>
    IAzureServiceBusQueueConfigurator AddSenderAndConsumer<TMessage, TConsumer>(string queueName, AzureServiceBusConsumerOptions options, params TimeSpan[] retries)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>;
}
