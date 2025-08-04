using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Quee.Interfaces;
using Quee.Services;

namespace Quee.AzureServiceBus;

/// <summary>
/// Handles setting up the senders and consumers for queuing messages into the Service Bus
/// </summary>
/// <param name="services">Service collection to register in</param>
/// <param name="connectionString">Service Bus Connection String</param>
internal sealed class AzureServiceBusQueueConfigurator(IServiceCollection services, string connectionString)
    : IAzureServiceBusQueueConfigurator
{
    /// <inheritdoc />
    public IQueueConfigurator AddQueueConsumer<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        services.AddTransient<IConsumer<TMessage>, TConsumer>();
        services.AddHostedService(provider =>
        {
            return new AzureServiceBusQueueConsumer<TMessage>(
                connectionString,
                queueName,
                provider.GetRequiredService<ILogger<AzureServiceBusQueueConsumer<TMessage>>>(),
                provider.GetRequiredService<IConsumer<TMessage>>(),
                provider.GetService<IQueueEventTrackingService>()); // Service optional depending on if dev registered it for use
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueSender<TMessage>(string queueName, params TimeSpan[] retries) where TMessage : class
    {
        services.AddScoped<IQueueSender<TMessage>>(provider =>
        {
            return new AzureServiceBusQueueSender<TMessage>(
                connectionString,
                queueName,
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
        services.RemoveAll<IQueueEventTrackingService>();
        services.AddSingleton<IQueueEventTrackingService>((provider) =>
        {
            return new QueueEventTrackingService(maximumMessagesPerQueue, provider.GetRequiredService<ILogger<QueueEventTrackingService>>());
        });
        return this;
    }
}
