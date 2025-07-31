using Microsoft.AspNetCore.Mvc;
using Quee.Interfaces;
using Quee.WebApp.Quee.Commands;

namespace Quee.WebApp.Controllers;

[ApiController, Route("api/send-message")]
public class MessageController(IQueueSender<LogMessageCommand> sender)
    : ControllerBase
{
    public async Task<ActionResult> GetAsync(CancellationToken cancellationToken)
    {
        await sender.SendMessageAsync(new LogMessageCommand("Super duper awesome!"), cancellationToken);
        return Ok();
    }
}
