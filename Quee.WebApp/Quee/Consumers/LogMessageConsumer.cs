using Quee.Interfaces;
using Quee.WebApp.Quee.Commands;

namespace Quee.WebApp.Quee.Consumers;

public class LogMessageConsumer(ILogger<LogMessageConsumer> logger)
    : IConsumer<LogMessageCommand>
{
    public Task ConsumeAsync(LogMessageCommand message, CancellationToken cancellationToken)
    {
        throw new NullReferenceException("Null because I feel like it");
        logger.LogInformation(message.message, cancellationToken);
        return Task.CompletedTask;
    }

    public Task ConsumeFaultAsync(IFault<LogMessageCommand> message, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Failed to log message {message.Payload.message} because of an exception.", cancellationToken);
        return Task.CompletedTask;
    }
}
