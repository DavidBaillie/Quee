using Microsoft.AspNetCore.Mvc;
using Quee.Interfaces;
using Quee.WebApp.Quee.Commands;

namespace Quee.WebApp.Controllers;

[ApiController]
public class MessageController(IQueueSender<LogMessageCommand> messageSender, IQueueSender<FailMessageCommand> failSender)
    : ControllerBase
{
    [HttpGet, Route("api/send-message")]
    public async Task<ActionResult> SendMessageAsync(CancellationToken cancellationToken)
    {
        await messageSender.SendMessageAsync(new LogMessageCommand("Super duper awesome!"), cancellationToken);
        return Ok();
    }

    [HttpGet, Route("api/send-failure")]
    public async Task<ActionResult> SendFailureAsync(CancellationToken cancellationToken)
    {
        await failSender.SendMessageAsync(new FailMessageCommand(new NullReferenceException($"Somtin null")), cancellationToken);
        return Ok();
    }
}
