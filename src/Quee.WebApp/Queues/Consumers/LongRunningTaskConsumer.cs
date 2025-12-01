using Quee.WebApp.Queues.Commands;

namespace Quee.WebApp.Queues.Consumers;

/// <summary>
/// Generic consumer that delays completion for a set number of milliseconds to simulate some asyncronous activity
/// </summary>
public class LongRunningTaskConsumer(ILogger<LongRunningTaskConsumer> logger)
    : IConsumer<LongRunningTaskCommand>
{
    /// <summary>
    /// Consumes the delay message, causing a delay of <see cref="LongRunningTaskCommand.milisecondsToWait"/> milliseconds
    /// </summary>
    /// <param name="message">Message with delay time</param>
    /// <param name="cancellationToken">Process token</param>
    public async Task ConsumeAsync(Message<LongRunningTaskCommand> message, CancellationToken cancellationToken)
    {
        await Task.Delay(message.Payload.milisecondsToWait, cancellationToken);
        logger.LogInformation("Completed long running task");
    }

    /// <summary>
    /// Consumes the fault of the message not delaying correctly? Should never really happen. 
    /// </summary>
    /// <param name="fault">Faulted message</param>
    /// <param name="cancellationToken">Process token</param>
    public Task ConsumeFaultAsync(FaultMessage<LongRunningTaskCommand> fault, CancellationToken cancellationToken)
    {
        // Should never run
        return Task.CompletedTask;
    }
}
