using FitApp.Application.Features.BodyMeasurements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/body-measurements")]
public class BodyMeasurementsController : ControllerBase
{
    private readonly ISender _sender;

    public BodyMeasurementsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List()
        => Ok(await _sender.Send(new GetBodyMeasurementsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(SaveBodyMeasurementRequest request)
    {
        var id = await _sender.Send(new CreateBodyMeasurementCommand(request));
        return Created($"/api/body-measurements/{id}", new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SaveBodyMeasurementRequest request)
    {
        await _sender.Send(new UpdateBodyMeasurementCommand(id, request));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteBodyMeasurementCommand(id));
        return NoContent();
    }
}
