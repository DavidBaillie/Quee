using Microsoft.Extensions.DependencyInjection;
using Quee.Interfaces;
using Quee.Services.Common;

namespace Quee.Services.AzureServiceBus;

/// <summary>
/// Sets up the queue to run through Azure Service Bus messaging 
/// </summary>
internal class ServiceBusQueueHandlerConfigurator(string connectionString)
    : IQueueHandlerConfigurator
{
    /// <inheritdoc/>
    public void SetupBuilder(IServiceCollection services, QueueConfigurator configurator)
    {
        throw new NotImplementedException();
    }
}
