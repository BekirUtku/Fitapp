using FitApp.Application.Features.WeeklyPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/weekly-plans")]
public class WeeklyPlansController : ControllerBase
{
    private readonly ISender _sender;

    public WeeklyPlansController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List()
        => Ok(await _sender.Send(new GetWeeklyPlansQuery()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _sender.Send(new GetWeeklyPlanByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create(CreateWeeklyPlanRequest request)
    {
        var id = await _sender.Send(new CreateWeeklyPlanCommand(request));
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateWeeklyPlanRequest request)
    {
        await _sender.Send(new UpdateWeeklyPlanCommand(id, request));
        return NoContent();
    }

    /// <summary>Bir güne şablon atar; TemplateId null ise o gün dinlenme günü olur.</summary>
    [HttpPut("{id:guid}/days")]
    public async Task<IActionResult> AssignDay(Guid id, AssignDayRequest request)
    {
        await _sender.Send(new AssignDayCommand(id, request));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteWeeklyPlanCommand(id));
        return NoContent();
    }
}
