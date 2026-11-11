using FitApp.Application.Features.Exercises;
using FitApp.Application.Features.Exercises.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

/// <summary>Sistem hareketleri yönetimi — yalnızca Admin.</summary>
[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin/exercises")]
public class AdminExercisesController : ControllerBase
{
    private readonly ISender _sender;

    public AdminExercisesController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(CreateExerciseRequest request)
    {
        var id = await _sender.Send(new CreateExerciseCommand(request, AsSystem: true));
        return Created($"/api/exercises/{id}", new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateExerciseRequest request)
    {
        await _sender.Send(new UpdateExerciseCommand(id, request));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteExerciseCommand(id));
        return NoContent();
    }
}
