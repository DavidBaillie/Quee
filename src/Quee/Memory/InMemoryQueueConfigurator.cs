using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Quee.Extensions;
using Quee.Interfaces;
using Quee.QueueOptions;

namespace Quee.Memory;

/// <summary>
/// Handles configuration for the in-memory queue based services
/// </summary>
internal class InMemoryQueueConfigurator
    : IInMemoryQueueConfigurator
{
    private readonly IServiceCollection services;

    private readonly bool allowRetries = true;


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
    public IQueueConfigurator AddMessageTracker(int maximumMessagesPerQueue = 100000)
    {
        services.AddQueeMessageTracker(maximumMessagesPerQueue);
        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddSender<TMessage>(string queueName, params TimeSpan[] retries)
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
    public IQueueConfigurator AddConsumer<TMessage, TConsumer>(string queueName)
        where TConsumer : class, IConsumer<TMessage>
        where TMessage : class
    {
        services.RemoveAll<IConsumer<TMessage>>();
        services.RemoveAll<InMemoryQueueConsumer<TMessage>>();

        services.AddScoped<IConsumer<TMessage>, TConsumer>();
        services.AddHostedService((provider) =>
        {
            return new InMemoryQueueConsumer<TMessage>(
                queueName,
                100,
                provider.GetRequiredService<IServiceScopeFactory>(),
                provider.GetRequiredService<IMemoryQueue>(),
                provider.GetRequiredService<ILogger<InMemoryQueueConsumer<TMessage>>>(),
                provider.GetService<IQueueEventTrackingService>());
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddSenderAndConsumer<TMessage, TConsumer>(string queueName, params TimeSpan[] retries)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        AddSender<TMessage>(queueName, retries);
        AddConsumer<TMessage, TConsumer>(queueName);
        return this;
    }
}
