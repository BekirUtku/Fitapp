using FitApp.Application.Features.PersonalRecords;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/personal-records")]
public class PersonalRecordsController : ControllerBase
{
    private readonly ISender _sender;

    public PersonalRecordsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> Current()
        => Ok(await _sender.Send(new GetPersonalRecordsQuery()));

    [HttpGet("exercise/{exerciseId:guid}")]
    public async Task<IActionResult> History(Guid exerciseId)
        => Ok(await _sender.Send(new GetExercisePrHistoryQuery(exerciseId)));
}
