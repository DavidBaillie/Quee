using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Quee;

internal class InMemoryQueueConsumer<TMessage>(
    string queueName,
    Channel<InMemoryMessage<TMessage>> channel,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<InMemoryQueueConsumer<TMessage>> logger,
    IQueueEventTrackingService? trackingService)
    : BackgroundService
    where TMessage : class
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancallationToken)
    {
        // Wait on the channel to have a message ready for us to read
        // Pass the cancellation token in to support cancellation of the process when needed.
        while (await channel.Reader.WaitToReadAsync(cancallationToken))
        {
            // Load the next message from the channel
            var message = await channel.Reader.ReadAsync(cancallationToken);

            // If the next message in the channel cannot be consumed yet, queue it to the back of the 
            // channel to be processed in the future when the delay has expired.
            // This allows other messages queued after the message to be consumed while this message's 
            // delay is waited for.
            if (message.ProcessNotBefore > DateTime.UtcNow)
            {
                await channel.Writer.WriteAsync(message, cancallationToken);
                continue;
            }
            
            // Define the start of a scope for the hosted service
            using var scope = serviceScopeFactory.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<TMessage>>();

            try
            {
                // Try to consume the message, catch any exception that might prevent it
                await consumer.ConsumeAsync(new Message<TMessage>()
                {
                    Payload = message.Payload,
                }, cancallationToken);
                
                // If the message raised an exception this will not trigger. 
                // In effect this means that a message which faults is not considered to have been "received"
                trackingService?.RecordReceivedMessage(queueName, message.Payload);
            }
            catch (Exception ex)
            {
                // Exception raised in the consumer, move to error processing.
                // Either a retry or a faul should occur.
                await HandleFailureAsync(consumer, message, ex, cancallationToken);
            }
        }
    }

    /// <summary>
    /// Consumption of the <paramref name="message"/> encountered a failure, process the <paramref name="exception"/> into either a retry or a fault
    /// </summary>
    /// <param name="message">Message that failed</param>
    /// <param name="exception">Exception encountered during consumption</param>
    /// <param name="cancellationToken">Process token</param>
    private async Task HandleFailureAsync(IConsumer<TMessage> consumer, InMemoryMessage<TMessage> message, Exception exception, CancellationToken cancellationToken)
    {
        // Ran out of retries, move the message into the fault consumer and allow the user to define what happens now that the message has failed
        if (message.RetryDelays is null || message.RetryNumber >= message.RetryDelays.Length)
        {
            try
            {
                // Fault conditions should always be tracked even when the consuming Fault handler fails. 
                // This is different from the "received" event which only triggers when a message is consumed without fault.
                trackingService?.RecordFaultedMessage(queueName, message.Payload);

                await consumer.ConsumeFaultAsync(new InMemoryFaultMessage<TMessage>()
                {
                    Payload = message.Payload,
                    SourceExceptions = [.. message.RetryExceptions, exception]
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                // If the fault dies we log it, dev shouldn't let this happen generally
                logger.LogError(ex, "In-Memory Consumer for {MessageType} invoked the fault consumer and encountered an exception.",
                    typeof(TMessage).GetType().Name);
            }

            return;
        }

        // Retry allowed, queue for another attempt
        await channel.Writer.WriteAsync(new InMemoryMessage<TMessage>()
        {
            Payload = message.Payload,
            ProcessNotBefore = DateTime.UtcNow + message.RetryDelays[message.RetryNumber],
            TargetQueue = message.TargetQueue,
            RetryDelays = message.RetryDelays,
            RetryNumber = message.RetryNumber + 1,
            RetryExceptions = [.. message.RetryExceptions, exception]
        }, cancellationToken);
    }
}