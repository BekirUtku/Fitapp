using FitApp.Application.Features.Sessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISender _sender;

    public SessionsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _sender.Send(new GetSessionsQuery(from, to)));

    /// <summary>Smart Resume: tamamlanmamış oturum varsa döner, yoksa 204.</summary>
    [HttpGet("active")]
    public async Task<IActionResult> Active()
    {
        var active = await _sender.Send(new GetActiveSessionQuery());
        return active is null ? NoContent() : Ok(active);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _sender.Send(new GetSessionByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Start(StartSessionRequest request)
    {
        var detail = await _sender.Send(new StartSessionCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = detail.Id }, detail);
    }

    [HttpPost("{id:guid}/exercises")]
    public async Task<IActionResult> AddExercise(Guid id, AddSessionExerciseRequest request)
        => Ok(await _sender.Send(new AddSessionExerciseCommand(id, request)));

    [HttpPut("{id:guid}/finish")]
    public async Task<IActionResult> Finish(Guid id, [FromBody] FinishSessionBody? body)
        => Ok(await _sender.Send(new FinishSessionCommand(id, body?.Note)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteSessionCommand(id));
        return NoContent();
    }
}

public record FinishSessionBody(string? Note);
