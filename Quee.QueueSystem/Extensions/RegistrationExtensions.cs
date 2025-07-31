using Microsoft.Extensions.DependencyInjection;
using Quee.Interfaces;
using Quee.Services.AzureServiceBus;

namespace Quee.Extensions;

/// <summary>
/// Allows for registering the implementation of the queue system
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection QueeWithAzureServiceBus(this IServiceCollection services, string connectionString, Action<IQueueConfigurator>? configuration = null)
    {
        // 
        IQueueConfigurator configurator = new AzureServiceBusQueueConfigurator(services, connectionString);
        configuration?.Invoke(configurator);

        return services;
    }
}
