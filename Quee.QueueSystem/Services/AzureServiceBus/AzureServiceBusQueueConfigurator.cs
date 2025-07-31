using Microsoft.Extensions.DependencyInjection;
using Quee.Interfaces;

namespace Quee.Services.AzureServiceBus;

/// <summary>
/// Handles setting up the senders and consumers for queuing messages into the Service Bus
/// </summary>
/// <param name="services">Service collection to register in</param>
/// <param name="connectionString">Service Bus Connection String</param>
internal sealed class AzureServiceBusQueueConfigurator(IServiceCollection services, string connectionString)
    : IQueueConfigurator
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
                provider.GetRequiredService<IConsumer<TMessage>>());
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueSender<TMessage>(string queueName) where TMessage : class
    {
        services.AddScoped<IQueueSender<TMessage>, AzureServiceBusQueueSender<TMessage>>(provider =>
        {
            return new AzureServiceBusQueueSender<TMessage>(connectionString, queueName);
        });
        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueProcessors<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        return AddQueueSender<TMessage>(queueName)
            .AddQueueConsumer<TMessage, TConsumer>(queueName);
    }
}
