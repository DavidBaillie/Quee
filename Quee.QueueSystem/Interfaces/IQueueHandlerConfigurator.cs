using Microsoft.Extensions.DependencyInjection;

namespace Quee.Interfaces;

/// <summary>
/// Defines how the internal implementation of the queue will be handled when messages are sent and received
/// </summary>
public interface IQueueHandlerConfigurator
{
    /// <summary>
    /// Given the service collection and configurator, setup the required processes and dependencies to handle 
    /// running the sending and receiving events of the queue.
    /// </summary>
    /// <param name="services">Runtime service collection</param>
    /// <param name="configurator">User configuration for the </param>
    void SetupBuilder(IServiceCollection services, IQueueConfigurator configurator);
}
