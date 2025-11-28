using Azure.Messaging.ServiceBus.Administration;

namespace Quee.AzureServiceBus.Services;

internal static class AzureServiceBusQueueManager
{
    /// <summary>
    /// Attempts to determine if the provided queue exists in the remote service bus, and if missing it attempts to create it.
    /// Should permissions be insufficient or some failure happen in the process, returns false to indicate the queue does not exist
    /// at the time of method completion.
    /// </summary>
    /// <param name="connectionString">Connection string to the service bus</param>
    /// <param name="queueName">Name of the queue to look for</param>
    /// <param name="cancellationToken">Process token</param>
    /// <returns>If the queue exists when the method returns</returns>
    public static async Task<bool> TryCreateQueueIfMissingAsync(string connectionString, string queueName, CancellationToken cancellationToken)
    {
        try
        {
            var client = new ServiceBusAdministrationClient(connectionString);

            if (!await client.QueueExistsAsync(queueName))
            {
                await client.CreateQueueAsync(queueName, cancellationToken);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
