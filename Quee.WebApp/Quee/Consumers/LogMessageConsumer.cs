using Quee.Interfaces;
using Quee.WebApp.Quee.Commands;

namespace Quee.WebApp.Quee.Consumers;

public class LogMessageConsumer(ILogger<LogMessageConsumer> logger)
    : IConsumer<LogMessageCommand>
{
    public Task ConsumeAsync(LogMessageCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation(message.message, cancellationToken);
        return Task.CompletedTask;
    }

    public Task ConsumeFaultAsync(LogMessageCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Failed to log message {message.message} because of an exception.", cancellationToken);
        return Task.CompletedTask;
    }
}
