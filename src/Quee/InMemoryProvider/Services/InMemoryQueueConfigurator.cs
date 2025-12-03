using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Handles configuration for the in-memory queue based services
/// </summary>
internal class InMemoryQueueConfigurator(IServiceCollection services)
        : IInMemoryQueueConfigurator
{
    private readonly bool allowRetries = true;
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

    /// <inheritdoc />
    public IQueueConfigurator DisableRetryPolicy()
    {
        services.RemoveAll<QueueRetryOptions>();
        services.AddTransient(_ => new QueueRetryOptions() { AllowRetries = false });
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
        // DI Container already has a sender registered for this Message type
        if (!registrationTracker.TryAdd(GetTypeKey(services, typeof(IQueueSender<TMessage>)), 0))
            return this;

        AddChannelForMessage<TMessage>();
        
        services.TryAddTransient<IQueueSender<TMessage>>((provider) =>
        {
            return new InMemoryQueueSender<TMessage>(
                queueName,
                provider.GetRequiredService<QueueRetryOptions>(),
                allowRetries ? retries : [], // Optionally disable the retries provided depending on the disable invocation
                provider.GetRequiredService<Channel<InMemoryMessage<TMessage>>>(),
                provider.GetService<IQueueEventTrackingService>()); 
        });

        return this;
    }

    /// <inheritdoc />
    public IQueueConfigurator AddConsumer<TMessage, TConsumer>(string queueName)
        where TConsumer : class, IConsumer<TMessage>
        where TMessage : class
    {
        // DI Container already has a consumer registered for this Message type
        if (!registrationTracker.TryAdd(GetTypeKey(services, typeof(IConsumer<TMessage>)), 0))
            return this;

        AddChannelForMessage<TMessage>();

        services.TryAddScoped<IConsumer<TMessage>, TConsumer>();
        services.AddHostedService((provider) =>
        {
            return new InMemoryQueueConsumer<TMessage>(
                queueName,
                provider.GetRequiredService<Channel<InMemoryMessage<TMessage>>>(),
                provider.GetRequiredService<IServiceScopeFactory>(),
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

    /// <summary>
    /// Registers a <see cref="Channel"/> for the message type <typeparamref name="TMessage"/> to use when reading or writing
    /// </summary>
    /// <typeparam name="TMessage">Type of message for the channel to send/receive</typeparam>
    private void AddChannelForMessage<TMessage>() 
        where TMessage : class
    {
        // Adds the singleton for the communication channel once to the DI container
        services.TryAddSingleton(
            _ => Channel.CreateUnbounded<InMemoryMessage<TMessage>>(
                new UnboundedChannelOptions()
                {
                    AllowSynchronousContinuations = false,  // Disable this because we don't want something like an API call to hang during a continious action
                    SingleReader = true,                    // Only one hosted service should be running at a time, disable the ability for multiple to read
                    SingleWriter = false                    // In theory everyone can write at the same time, this can be enabled
                }));
    }
}
