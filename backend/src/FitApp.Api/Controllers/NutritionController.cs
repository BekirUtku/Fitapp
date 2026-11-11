using FitApp.Application.Features.Nutrition;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nutrition")]
public class NutritionController : ControllerBase
{
    private readonly ISender _sender;

    public NutritionController(ISender sender) => _sender = sender;

    /// <summary>Belirli bir günün log + hedef + kalan makro özeti.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] DateOnly? date)
        => Ok(await _sender.Send(new GetNutritionSummaryQuery(date ?? DateOnly.FromDateTime(DateTime.UtcNow))));

    [HttpPut("logs")]
    public async Task<IActionResult> UpsertLog(SaveLogRequest request)
    {
        await _sender.Send(new UpsertNutritionLogCommand(request));
        return NoContent();
    }

    [HttpPost("logs/water")]
    public async Task<IActionResult> AddWater(AddWaterRequest request)
        => Ok(new { waterMl = await _sender.Send(new AddWaterCommand(request)) });

    [HttpGet("targets")]
    public async Task<IActionResult> GetTarget()
        => Ok(await _sender.Send(new GetCurrentTargetQuery()));

    [HttpPut("targets")]
    public async Task<IActionResult> SetTarget(SaveTargetRequest request)
    {
        await _sender.Send(new SetTargetCommand(request));
        return NoContent();
    }
}
