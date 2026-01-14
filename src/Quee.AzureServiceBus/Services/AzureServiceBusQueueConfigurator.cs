using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quee.AzureServiceBus.Interfaces;
using System.Collections.Concurrent;

namespace Quee.AzureServiceBus.Services;

/// <summary>
/// Handles setting up the senders and consumers for queuing messages into the Service Bus
/// </summary>
/// <param name="services">Service collection to register in</param>
/// <param name="connectionString">Service Bus Connection String</param>
internal sealed class AzureServiceBusQueueConfigurator
    : IAzureServiceBusQueueConfigurator
{
    private readonly IServiceCollection services;
    private readonly string connectionString;

    private readonly ConcurrentDictionary<string, byte> registrationTracker = new();

    /// <summary>
    /// Takes the DI Container instance and combines it with the type information of the provided type. 
    /// Allows for a unique value for each registered type per possible container. 
    /// </summary>
    /// <param name="services">Services to add the Type to</param>
    /// <param name="type">Type that might be added to the container</param>
    /// <returns>unique string for the type that would be inserted into the container.</returns>
    private static string GetTypeKey(IServiceCollection services, Type type)
        => $"{services.GetHashCode()}:{type.FullName}";

    public AzureServiceBusQueueConfigurator(IServiceCollection services, string connectionString, bool allowAdminManagement)
    {
        this.services = services;
        this.connectionString = connectionString;

        services.RemoveAll<QueueRetryOptions>();
        services.AddTransient(_ => new QueueRetryOptions() { AllowRetries = true });

        // Register the Client as a singleton for all threads/tasks to share as a common pooled resource
        services.TryAddSingleton(new ServiceBusClient(connectionString));

        // Register the queue sender manager as a shared resource that allows a single sender to be shared across multiple threads
        services.TryAddSingleton<IAzureServiceBusQueueSenderManager, AzureServiceBusQueueSenderManager>();

        // If the runtime is allowed to manage queues, enable the admin client for use
        if (allowAdminManagement)
            services.TryAddSingleton(new ServiceBusAdministrationClient(connectionString));
    }

    /// <inheritdoc />
    public IAzureServiceBusQueueConfigurator AddQueueConsumerOptions(string queueName, ConsumerOptions options)
    {
        // Always override the provided value to enforce the associated queue name
        options.TargetQueue = queueName;
        services.AddTransient(_ => options);

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddConsumer<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        return AddConsumer<TMessage, TConsumer>(queueName, null);
    }

    /// <inheritdoc />
    public IAzureServiceBusQueueConfigurator AddConsumer<TMessage, TConsumer>(string queueName, ConsumerOptions? options)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        // DI Container already has a sender registered for this Message type
        if (!registrationTracker.TryAdd(GetTypeKey(services, typeof(IConsumer<TMessage>)), 0))
            return this;

        // If options were provided, register them
        if (options != null)
            services.AddTransient(_ => options);

        // Add the consumer from the user and a hosted service to invoke the consumer
        services.AddTransient<IConsumer<TMessage>, TConsumer>();
        services.AddHostedService(provider =>
        {
            return new AzureServiceBusQueueConsumer<TMessage>(
                queueName,
                provider);
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddSender<TMessage>(string queueName, params TimeSpan[] retries) where TMessage : class
    {
        // DI Container already has a sender registered for this Message type
        if (!registrationTracker.TryAdd(GetTypeKey(services, typeof(IQueueSender<TMessage>)), 0))
            return this;

        services.AddScoped<IQueueSender<TMessage>>(provider =>
        {
            return new AzureServiceBusQueueSender<TMessage>(
                provider.GetRequiredService<IAzureServiceBusQueueSenderManager>(),
                queueName,
                provider.GetRequiredService<QueueRetryOptions>(),
                provider.GetService<IQueueEventTrackingService>(),
                retries);
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddSenderAndConsumer<TMessage, TConsumer>(string queueName, params TimeSpan[] retries)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        return AddSenderAndConsumer<TMessage, TConsumer>(queueName, null, retries);
    }

    /// <inheritdoc />
    public IAzureServiceBusQueueConfigurator AddSenderAndConsumer<TMessage, TConsumer>(string queueName, ConsumerOptions? options, params TimeSpan[] retries)
    where TMessage : class
    where TConsumer : class, IConsumer<TMessage>
    {
        AddSender<TMessage>(queueName, retries);
        AddConsumer<TMessage, TConsumer>(queueName, options);
        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddMessageTracker(int maximumMessagesPerQueue = 100_000)
    {
        services.AddQueeMessageTracker(maximumMessagesPerQueue);
        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator DisableRetryPolicy()
    {
        services.RemoveAll<QueueRetryOptions>();
        services.AddTransient(_ => new QueueRetryOptions() { AllowRetries = false });

        return this;
    }
}
