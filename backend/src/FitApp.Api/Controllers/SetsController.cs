using FitApp.Application.Features.Sessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class SetsController : ControllerBase
{
    private readonly ISender _sender;

    public SetsController(ISender sender) => _sender = sender;

    [HttpPost("session-exercises/{sessionExerciseId:guid}/sets")]
    public async Task<IActionResult> AddSet(Guid sessionExerciseId, AddSetRequest request)
        => Ok(await _sender.Send(new AddSetCommand(sessionExerciseId, request)));

    [HttpPut("sets/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSetRequest request)
        => Ok(await _sender.Send(new UpdateSetCommand(id, request)));

    [HttpPut("sets/{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
        => Ok(await _sender.Send(new SetCompletionCommand(id, Completed: true)));

    /// <summary>✓ onayını geri alır (undo).</summary>
    [HttpPut("sets/{id:guid}/uncomplete")]
    public async Task<IActionResult> Uncomplete(Guid id)
        => Ok(await _sender.Send(new SetCompletionCommand(id, Completed: false)));

    [HttpDelete("sets/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteSetCommand(id));
        return NoContent();
    }
}
