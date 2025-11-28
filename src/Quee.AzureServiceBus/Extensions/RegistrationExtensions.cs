using Microsoft.Extensions.DependencyInjection;
using Quee.AzureServiceBus.Models;
using Quee.AzureServiceBus.Services;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

public static class RegistrationExtensions
{
    /// <summary>
    /// Introduce Quee with Azure Service Bus as the external queuing provider.
    /// </summary>
    /// <param name="services">Service collection for depenency injection</param>
    /// <param name="connectionString">Connection string to the service bus</param>
    /// <param name="configuration">Options object for configuration</param>
    /// <returns>Cascading rerence to <see cref="IServiceCollection"/></returns>
    public static IServiceCollection QueeWithAzureServiceBus(
        this IServiceCollection services,
        string connectionString,
        Action<IAzureServiceBusQueueConfigurator>? configuration = null)
    {
        IAzureServiceBusQueueConfigurator configurator = new AzureServiceBusQueueConfigurator(services, connectionString);
        configuration?.Invoke(configurator);

        return services;
    }
}
