using FitApp.Application.Features.Catalog;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class CatalogController : ControllerBase
{
    private readonly ISender _sender;

    public CatalogController(ISender sender) => _sender = sender;

    [HttpGet("muscle-groups")]
    public async Task<IActionResult> GetMuscleGroups()
        => Ok(await _sender.Send(new GetMuscleGroupsQuery()));

    [HttpGet("equipment")]
    public async Task<IActionResult> GetEquipment()
        => Ok(await _sender.Send(new GetEquipmentQuery()));
}
