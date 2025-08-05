using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Quee.Interfaces;
using Quee.QueueOptions;
using Quee.Services;

namespace Quee.Memory;

/// <summary>
/// Handles configuration for the in-memory queue based services
/// </summary>
internal class InMemoryQueueConfigurator
    : IInMemoryQueueConfigurator
{
    private readonly IServiceCollection services;

    private bool allowRetries = true;


    public InMemoryQueueConfigurator(IServiceCollection services)
    {
        this.services = services;
        services.AddSingleton<IMemoryQueue, InMemoryQueue>();

        services.RemoveAll<QueueRetryOptions>();
        services.AddTransient((_) => new QueueRetryOptions() { AllowRetries = true });
    }

    /// <inheritdoc />
    public IQueueConfigurator DisableRetryPolicy()
    {
        services.RemoveAll<QueueRetryOptions>();
        services.AddTransient((_) => new QueueRetryOptions() { AllowRetries = false });
        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueMessageTracker(int maximumMessagesPerQueue = 100000)
    {
        services.RemoveAll<IQueueMonitor>();
        services.RemoveAll<IQueueEventTrackingService>();

        services.AddTransient<IQueueMonitor, QueueMonitor>();
        services.AddSingleton<IQueueEventTrackingService>((provider) =>
        {
            return new QueueEventTrackingService(
                maximumMessagesPerQueue,
                provider.GetRequiredService<ILogger<QueueEventTrackingService>>());
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueSender<TMessage>(string queueName, params TimeSpan[] retries)
        where TMessage : class
    {
        services.RemoveAll<IQueueSender<TMessage>>();

        services.AddTransient<IQueueSender<TMessage>>((provider) =>
        {
            return new InMemoryQueueSender<TMessage>(
                queueName,
                provider.GetRequiredService<IMemoryQueue>(),
                provider.GetRequiredService<QueueRetryOptions>(),
                provider.GetService<IQueueEventTrackingService>(),
                allowRetries ? retries : []); // Optionally disable the retries provided depending on the disable invocation
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddQueueConsumer<TMessage, TConsumer>(string queueName)
        where TConsumer : class, IConsumer<TMessage>
        where TMessage : class
    {
        services.RemoveAll<IConsumer<TMessage>>();
        services.RemoveAll<InMemoryQueueConsumer<TMessage>>();

        services.AddTransient<IConsumer<TMessage>, TConsumer>();
        services.AddHostedService((provider) =>
        {
            return new InMemoryQueueConsumer<TMessage>(
                queueName,
                100,
                provider.GetRequiredService<IConsumer<TMessage>>(),
                provider.GetRequiredService<IMemoryQueue>(),
                provider.GetRequiredService<ILogger<InMemoryQueueConsumer<TMessage>>>(),
                provider.GetService<IQueueEventTrackingService>());
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
}
