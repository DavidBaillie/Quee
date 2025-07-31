using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quee.Interfaces;
using Quee.Services.AzureServiceBus;
using Quee.Services.Common;

namespace QueueUtility.QueueSystem.Extensions;

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
    public static IServiceCollection AddQuee(this IServiceCollection services, Action<IQueueConfigurator>? configuration = null)
    {
        // 
        IQueueConfigurator configurator = new QueueConfigurator();
        configuration?.Invoke(configurator);

        configurator.QueueHandler.SetupBuilder(services, configurator);

        services.TryAddScoped((_) => configurator);
        return services;
    }

    /// <summary>
    /// Adds the azure service bus handler to the container to allow the queue to be processed through Azure Service Bus
    /// </summary>
    /// <param name="configurator">Configurator to add service bus to</param>
    /// <param name="connectionString">Connection string to connect with</param>
    /// <returns>Cascading reference</returns>
    public static IQueueConfigurator AddAzureServiceBus(this IQueueConfigurator configurator, string connectionString)
    {
        configurator.QueueHandler = new ServiceBusQueueHandlerConfigurator(connectionString);
        return configurator;
    }
}
