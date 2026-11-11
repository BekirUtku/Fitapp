using FitApp.Application.Features.Templates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    private readonly ISender _sender;

    public TemplatesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List()
        => Ok(await _sender.Send(new GetTemplatesQuery()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _sender.Send(new GetTemplateByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create(SaveTemplateRequest request)
    {
        var id = await _sender.Send(new CreateTemplateCommand(request));
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SaveTemplateRequest request)
    {
        await _sender.Send(new UpdateTemplateCommand(id, request));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteTemplateCommand(id));
        return NoContent();
    }
}
