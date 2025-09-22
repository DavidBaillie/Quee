using Microsoft.Extensions.DependencyInjection;
using Quee.Interfaces;
using Quee.WebApp.Queues.Commands;

namespace Quee.Tests.Integration.Tests;


internal class LongRunningTaskTests : IntegrationTestBase
{
    [Test]
    public async Task WhenQueueLongRunningTaskWillBeConsumed()
    {
        // Arrange
        using var scope = CreateScope();
        var monitor = scope.ServiceProvider.GetRequiredService<IQueueMonitor>();

        var sourceMessage = new LongRunningTaskCommand(500);

        // Act
        await scope.ServiceProvider.GetRequiredService<IQueueSender<LongRunningTaskCommand>>()
            .SendMessageAsync(sourceMessage, CancellationToken.None);

        // Assert
        var sentMessage = await monitor.WaitForMessageToSend<LongRunningTaskCommand>(
            nameof(LongRunningTaskCommand),
            TimeSpan.FromSeconds(1),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(sentMessage != null, $"{nameof(LongRunningTaskCommand)} was requested to send into the Queue but no message was recorded as being sent.", sourceMessage);

        var consumedMessage = await monitor.WaitForMessageToReceive<LongRunningTaskCommand>(
            nameof(LongRunningTaskCommand),
            TimeSpan.FromSeconds(10),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(consumedMessage != null, $"{nameof(LongRunningTaskCommand)} was sent into the queue, but was never consumed.", sourceMessage);

        var faultMessage = await monitor.WaitForMessageToFault<LongRunningTaskCommand>(
            nameof(LongRunningTaskCommand),
            TimeSpan.FromMilliseconds(100),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(faultMessage == null, $"{nameof(LongRunningTaskCommand)} was found in the fault queue when it should have been consumed.", sourceMessage);
    }
}
