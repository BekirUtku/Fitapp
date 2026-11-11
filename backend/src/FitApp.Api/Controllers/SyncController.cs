using FitApp.Application.Features.Sync;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly ISender _sender;

    public SyncController(ISender sender) => _sender = sender;

    /// <summary>İstemcideki senkronize olmamış değişiklikleri (upsert/delete) toplu gönderir.</summary>
    [HttpPost("push")]
    public async Task<ActionResult<PushResponse>> Push(PushRequest request)
        => Ok(await _sender.Send(new PushChangesCommand(request)));

    /// <summary>since zamanından sonra değişen sunucu kayıtlarını çeker.</summary>
    [HttpGet("pull")]
    public async Task<ActionResult<PullResponse>> Pull([FromQuery] DateTime? since)
        => Ok(await _sender.Send(new PullChangesQuery(since)));
}
