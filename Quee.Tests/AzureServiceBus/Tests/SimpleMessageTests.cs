﻿using Microsoft.Extensions.DependencyInjection;
using Quee.Interfaces;
using Quee.Tests.Integration;
using Quee.Tests.Queues.Commands;

namespace Quee.Tests.AzureServiceBus.Tests;

internal class SimpleMessageTests : AzureServiceBusTestBase
{
    private const string QUEUE_NAME = "quee-test-sm";

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
            QUEUE_NAME,
            TimeSpan.FromSeconds(10),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(sentMessage != null, $"{nameof(SimpleMessageCommand)} was requested to send into the Queue but no message was recorded as being sent.", sourceMessage);

        var consumedMessage = await monitor.WaitForMessageToReceive<SimpleMessageCommand>(
            QUEUE_NAME,
            TimeSpan.FromSeconds(10),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(consumedMessage != null, $"{nameof(SimpleMessageCommand)} was sent into the queue, but was never consumed.", sourceMessage);

        var faultMessage = await monitor.WaitForMessageToFault<SimpleMessageCommand>(
            QUEUE_NAME,
            TimeSpan.FromMilliseconds(100),
            CancellationToken.None,
            x => x.Id == sourceMessage.Id);
        Assert.That(faultMessage == null, $"{nameof(SimpleMessageCommand)} was found in the fault queue when it should have been consumed.", sourceMessage);
    }
}
