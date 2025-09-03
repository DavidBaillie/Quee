namespace Quee.AzureServiceBus;

public class AzureServiceBusConsumerOptions
{
    /// <summary>
    /// Defines how many messages can be processed at the same by the consumer. The default value of 1
    /// will cause all messages to be consumed sequentially, meaning message #1 must complete before message #2 can begin. 
    /// Larger values than 1 will allow the consumer to process multiple messages as concurrent <see cref="Task"/> objects
    /// in the consumer.
    /// </summary>
    public int ConcurrencyLimit { get; set; } = 1;

    /// <summary>
    /// Determines how many messages the consumer will be allowed to load into memory before being processed. By default no
    /// messages will be prefetched from the Service Bus, however values greater than zero will allow the consumer to have the next
    /// message available to load from memory instead needing to wait for the next message to be fetched from the external Service Bus.
    /// <para />
    /// WARNING: The large you set this value, the larger the memory footprint. Please keep memory consumption in mind as you modify this setting.
    /// </summary>
    public int PrefetchLimit { get; set; } = 0;
}