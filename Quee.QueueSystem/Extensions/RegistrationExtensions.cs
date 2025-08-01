using Microsoft.Extensions.DependencyInjection;
using Quee.AzureServiceBus;
using Quee.Interfaces;

namespace Quee.Extensions;

/// <summary>
/// Allows for registering the implementation of the queue system
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// Introduce Quee with Azure Service Bus as the external queuing provider.
    /// </summary>
    /// <param name="services">Service collection for depenency injection</param>
    /// <param name="connectionString">Connection string to the service bus</param>
    /// <param name="configuration">Options object for configuration</param>
    /// <returns>Cascading reference</returns>
    public static IServiceCollection QueeWithAzureServiceBus(this IServiceCollection services, string connectionString, Action<IQueueConfigurator>? configuration = null)
    {
        IQueueConfigurator configurator = new AzureServiceBusQueueConfigurator(services, connectionString);
        configuration?.Invoke(configurator);

        return services;
    }
}
