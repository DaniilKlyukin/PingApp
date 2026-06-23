using Microsoft.AspNetCore.Mvc;
using PingApp.Application.Features.Devices;

namespace PingApp.Api.Controllers;

public class DevicesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAllowedDevices(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAllowedDevices.Query(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("statuses")]
    public async Task<ActionResult<List<GetGlobalDeviceStatuses.DeviceStatusDto>>> GetStatuses(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetGlobalDeviceStatuses.Query(), cancellationToken);
        return Ok(result);
    }
}