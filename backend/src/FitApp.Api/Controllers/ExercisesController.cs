using FitApp.Application.Features.Exercises;
using FitApp.Application.Features.Exercises.Commands;
using FitApp.Application.Features.Exercises.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/exercises")]
public class ExercisesController : ControllerBase
{
    private readonly ISender _sender;

    public ExercisesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? muscleGroup,
        [FromQuery] Guid? equipment,
        [FromQuery] string? search,
        [FromQuery] bool? customOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
        => Ok(await _sender.Send(new GetExercisesQuery(muscleGroup, equipment, search, customOnly, page, pageSize)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _sender.Send(new GetExerciseByIdQuery(id)));

    /// <summary>Harekete özel geçmiş: tarih bazlı setler, hacim, 1RM ve PR tarihçesi.</summary>
    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> History(Guid id)
        => Ok(await _sender.Send(new GetExerciseHistoryQuery(id)));

    /// <summary>Kullanıcının kendi özel hareketini ekler.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateExerciseRequest request)
    {
        var id = await _sender.Send(new CreateExerciseCommand(request, AsSystem: false));
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
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
