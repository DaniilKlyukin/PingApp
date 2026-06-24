using Microsoft.AspNetCore.Mvc;
using PingApp.Application.Features.Telegram;

namespace PingApp.Api.Controllers;

public class TelegramController : ApiControllerBase
{
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeTelegram.Command command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Message });
        }
        return Ok();
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeTelegram.Command command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { Error = result.Error.Message });
        }
        return Ok();
    }

    [HttpGet("subscriptions/{chatId}")]
    public async Task<ActionResult<List<GetTelegramSubscriptions.SubscriptionDto>>> GetSubscriptions(long chatId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTelegramSubscriptions.Query(chatId), cancellationToken);
        return Ok(result);
    }
}
