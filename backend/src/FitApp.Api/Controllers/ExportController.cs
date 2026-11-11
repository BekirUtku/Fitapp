using FitApp.Application.Features.Export;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly ISender _sender;

    public ExportController(ISender sender) => _sender = sender;

    /// <summary>Antrenman verisini dışa aktarır. format = json | csv (Excel/PDF sonraki turda).</summary>
    [HttpGet]
    public async Task<IActionResult> Export([FromQuery] string format = "json")
    {
        var result = await _sender.Send(new ExportWorkoutsQuery(format));
        return File(result.Content, result.ContentType, result.FileName);
    }
}
