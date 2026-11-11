using FitApp.Application.Features.Insights;
using FitApp.Application.Features.Progression;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class InsightsController : ControllerBase
{
    private readonly ISender _sender;

    public InsightsController(ISender sender) => _sender = sender;

    [HttpGet("insights/summary")]
    public async Task<IActionResult> Summary()
        => Ok(await _sender.Send(new GetInsightsSummaryQuery()));

    [HttpGet("insights/session/{id:guid}")]
    public async Task<IActionResult> Session(Guid id)
        => Ok(await _sender.Send(new GetSessionInsightsQuery(id)));

    [HttpGet("progression/suggestion")]
    public async Task<IActionResult> Suggestion([FromQuery] Guid exerciseId)
        => Ok(await _sender.Send(new GetProgressionSuggestionQuery(exerciseId)));

    [HttpGet("progression/warmup")]
    public async Task<IActionResult> Warmup([FromQuery] decimal workingWeight, [FromQuery] Guid? exerciseId)
        => Ok(await _sender.Send(new GetWarmupQuery(workingWeight, exerciseId)));
}
