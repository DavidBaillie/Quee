using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

/// <summary>
/// Allows for registering the implementation of the queue system
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// Introduce Quee using an in-memory provider to handle message communication
    /// </summary>
    /// <param name="services">Service collection for dependency injection</param>
    /// <param name="configuration">Options object for configuration</param>
    /// <returns>Cascading rerence to <see cref="IServiceCollection"/></returns>
    public static IServiceCollection QueeInMemory(
        this IServiceCollection services,
        Action<IInMemoryQueueConfigurator>? configuration = null)
    {
        IInMemoryQueueConfigurator configurator = new InMemoryQueueConfigurator(services);
        configuration?.Invoke(configurator);

        return services;
    }

    /// <summary>
    /// Adds the message tracking service into the send/receive system to allow you to monitor all messages sent and received by the local runtime.
    /// <para>CAUTION: This will impact performance and should only be added in development or test environments where tracking all messages is important.</para>
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to register monitor with</param>
    /// <param name="maximumMessagesPerQueue">The maxmimum number of messages that will be stored for each queue in local memory</param>
    /// <returns>Cascading rerence to <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddQueeMessageTracker(this IServiceCollection services, int maximumMessagesPerQueue = 100000)
    {
        services.RemoveAll<IQueueMonitor>();
        services.RemoveAll<IQueueEventTrackingService>();

        services.AddTransient<IQueueMonitor, QueueMonitor>();
        services.AddSingleton<IQueueEventTrackingService>((provider) =>
        {
            return new QueueEventTrackingService(maximumMessagesPerQueue);
        });

        return services;
    }
}
