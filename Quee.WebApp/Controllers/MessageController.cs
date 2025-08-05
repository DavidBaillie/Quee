using Microsoft.AspNetCore.Mvc;
using Quee.Interfaces;
using Quee.WebApp.Quee.Commands;

namespace Quee.WebApp.Controllers;

[ApiController]
public class MessageController(IQueueSender<LogMessageCommand> messageSender, IQueueSender<FailMessageCommand> failSender, IQueueMonitor queueMonitor)
    : ControllerBase
{
    [HttpGet, Route("api/send-message")]
    public async Task<ActionResult> SendMessageAsync(CancellationToken cancellationToken)
    {
        await messageSender.SendMessageAsync(new LogMessageCommand("Super duper awesome!"), cancellationToken);

        await queueMonitor.WaitForMessageToSend("LogMessage-Queue", TimeSpan.FromSeconds(30), cancellationToken, (LogMessageCommand message) =>
        {
            return message.Message == "Super duper awesome!";
        });

        await queueMonitor.WaitForMessageToReceive("LogMessage-Queue", TimeSpan.FromSeconds(30), cancellationToken, (LogMessageCommand message) =>
        {
            return message.Message == "Super duper awesome!";
        });

        return Ok();
    }

    [HttpGet, Route("api/send-failure")]
    public async Task<ActionResult> SendFailureAsync(CancellationToken cancellationToken)
    {

        await failSender.SendMessageAsync(new FailMessageCommand(new NullReferenceException($"Somtin null")), cancellationToken);

        await queueMonitor.WaitForMessageToSend("FailMessage-Queue", TimeSpan.FromSeconds(30), cancellationToken, (FailMessageCommand message) =>
        {
            return message.Exception.Message == "Somtin null";
        });

        await queueMonitor.WaitForMessageToFault("FailMessage-Queue", TimeSpan.FromSeconds(30), cancellationToken, (FailMessageCommand message) =>
        {
            return message.Exception.Message == "Somtin null";
        });

        return Ok();
    }
}
