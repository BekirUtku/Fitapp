using FitApp.Application.Features.Gamification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/gamification")]
public class GamificationController : ControllerBase
{
    private readonly ISender _sender;

    public GamificationController(ISender sender) => _sender = sender;

    [HttpGet("streak")]
    public async Task<IActionResult> Streak([FromQuery] int days = 120)
        => Ok(await _sender.Send(new GetStreakQuery(days)));

    [HttpGet("muscle-heatmap")]
    public async Task<IActionResult> Heatmap([FromQuery] int days = 30)
        => Ok(await _sender.Send(new GetMuscleHeatmapQuery(days)));

    [HttpGet("badges")]
    public async Task<IActionResult> Badges()
        => Ok(await _sender.Send(new GetBadgesQuery()));
}
