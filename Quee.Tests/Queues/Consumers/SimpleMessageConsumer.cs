using Microsoft.Extensions.Logging;
using Quee.Interfaces;
using Quee.Messages;
using Quee.Tests.Queues.Commands;

namespace Quee.Tests.Queues.Consumers;

internal class SimpleMessageConsumer(ILogger<SimpleMessageConsumer> logger)
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
