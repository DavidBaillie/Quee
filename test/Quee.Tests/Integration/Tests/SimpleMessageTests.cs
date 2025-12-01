using Microsoft.Extensions.DependencyInjection;
using Quee.WebApp.Queues.Commands;

namespace Quee.Tests.Integration.Tests;

internal class SimpleMessageTests : IntegrationTestBase
{
    [TestCase("Singular")]
    [TestCase("Something longer")]
    [TestCase("GARBLED_osajhd;SAODHC;SDCKnS:DKvcjnS:DC")]
    public async Task WhenMessageSentIsReceived(string message)
    {
        // Arrange
        using var scope = CreateScope();
        var monitor = scope.ServiceProvider.GetRequiredService<IQueueMonitor>();

        var sourceMessage = new SimpleMessageCommand(Guid.NewGuid(), message);

        // Act
        await scope.ServiceProvider.GetRequiredService<IQueueSender<SimpleMessageCommand>>()
            .SendMessageAsync(sourceMessage, CancellationToken.None);

        // Assert
        var sentMessage = await monitor.WaitForMessageToSend<SimpleMessageCommand>(
            nameof(SimpleMessageCommand),
            TimeSpan.FromSeconds(1),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(sentMessage != null, $"{nameof(SimpleMessageCommand)} was requested to send into the Queue but no message was recorded as being sent.", sourceMessage);

        var consumedMessage = await monitor.WaitForMessageToReceive<SimpleMessageCommand>(
            nameof(SimpleMessageCommand),
            TimeSpan.FromSeconds(10),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(consumedMessage != null, $"{nameof(SimpleMessageCommand)} was sent into the queue, but was never consumed.", sourceMessage);

        var faultMessage = await monitor.WaitForMessageToFault<SimpleMessageCommand>(
            nameof(SimpleMessageCommand),
            TimeSpan.FromMilliseconds(100),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(faultMessage == null, $"{nameof(SimpleMessageCommand)} was found in the fault queue when it should have been consumed.", sourceMessage);
    }
}
