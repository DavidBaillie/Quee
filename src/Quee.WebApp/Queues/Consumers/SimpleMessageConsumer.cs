using Quee.WebApp.Queues.Commands;

namespace Quee.WebApp.Queues.Consumers;

public class SimpleMessageConsumer(ILogger<SimpleMessageConsumer> logger)
    : IConsumer<SimpleMessageCommand>
{
    public Task ConsumeAsync(Message<SimpleMessageCommand> message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message {Message}", message.Payload.Message);
        return Task.CompletedTask;
    }

    public Task ConsumeFaultAsync(FaultMessage<SimpleMessageCommand> fault, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fauled when message received...weird");
        return Task.CompletedTask;
    }
}
