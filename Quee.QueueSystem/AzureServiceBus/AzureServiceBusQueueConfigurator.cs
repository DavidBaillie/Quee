using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Quee.Interfaces;
using Quee.QueueOptions;
using Quee.Services;

namespace Quee.AzureServiceBus;

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

    public AzureServiceBusQueueConfigurator(IServiceCollection services, string connectionString)
    {
        this.services = services;
        this.connectionString = connectionString;

        services.RemoveAll<QueueRetryOptions>();
        services.AddTransient((_) => new QueueRetryOptions() { AllowRetries = true });
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueConsumer<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        services.RemoveAll<IConsumer<TMessage>>();
        services.RemoveAll<AzureServiceBusQueueConsumer<TMessage>>();

        services.AddTransient<IConsumer<TMessage>, TConsumer>();
        services.AddHostedService(provider =>
        {
            return new AzureServiceBusQueueConsumer<TMessage>(
                connectionString,
                queueName,
                provider.GetRequiredService<ILogger<AzureServiceBusQueueConsumer<TMessage>>>(),
                provider.GetRequiredService<IServiceScopeFactory>(),
                provider.GetService<IQueueEventTrackingService>()); // Service optional depending on if dev registered it for use
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueSender<TMessage>(string queueName, params TimeSpan[] retries) where TMessage : class
    {
        services.RemoveAll<IConsumer<TMessage>>();

        services.AddScoped<IQueueSender<TMessage>>(provider =>
        {
            return new AzureServiceBusQueueSender<TMessage>(
                connectionString,
                queueName,
                provider.GetRequiredService<QueueRetryOptions>(),
                provider.GetService<IQueueEventTrackingService>(),
                retries);
        });
        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueProcessors<TMessage, TConsumer>(string queueName, params TimeSpan[] retries)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        AddQueueSender<TMessage>(queueName, retries);
        AddQueueConsumer<TMessage, TConsumer>(queueName);
        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueMessageTracker(int maximumMessagesPerQueue = 100_000)
    {
        services.RemoveAll<IQueueMonitor>();
        services.RemoveAll<IQueueEventTrackingService>();

        services.AddTransient<IQueueMonitor, QueueMonitor>();
        services.AddSingleton<IQueueEventTrackingService>((provider) =>
        {
            return new QueueEventTrackingService(maximumMessagesPerQueue);
        });
        return this;
    }

    public IQueueConfigurator DisableRetryPolicy()
    {
        services.RemoveAll<QueueRetryOptions>();
        services.AddTransient((_) => new QueueRetryOptions() { AllowRetries = false });

        return this;
    }
}
