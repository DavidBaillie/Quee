using Quee.Interfaces;
using Quee.WebApp.Quee.Commands;

namespace Quee.WebApp.Quee.Consumers;

public class FailMessageConsumer(ILogger<FailMessageConsumer> logger)
    : IConsumer<FailMessageCommand>
{
    public Task ConsumeAsync(FailMessageCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to fail - {Id}", Guid.NewGuid());
        throw message.Exception;
    }

    public Task ConsumeFaultAsync(IFault<FailMessageCommand> fault, CancellationToken cancellationToken)
    {
        logger.LogError("Failed successfully:\n{Exception}", string.Join("\n", fault.Exceptions));
        return Task.CompletedTask;
    }
}
