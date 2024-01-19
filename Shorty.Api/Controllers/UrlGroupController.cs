using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shorty.Api.Controllers.Contracts;
using Shorty.Application;

namespace Shorty.Api.Controllers;

[ApiController]
[Route("api")]
public class UrlGroupController : ControllerBase
{
    private readonly IMediator _mediator;

    public UrlGroupController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("owners/{ownerId}/url-groups")]
    public async Task<IActionResult> GetUrlGroupsAsync([FromRoute] string ownerId, [FromQuery] GetUrlGroupListRequest listRequest, CancellationToken cancellationToken)
    {
        var command = new GetUrlGroupListCommand(ownerId, listRequest.PageNumber, listRequest.PageSize);
        GetUrlGroupListCommandResult result = await _mediator.Send(command, cancellationToken);

        var responseList = result.Data.Select(d => new GetUrlGroupResponse(d.GroupId, d.Name, d.Description)).ToList();
        var response = new GetUrlGroupListResponse(responseList);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost("url-groups")]
    public async Task<IActionResult> PostUrlGroupAsync([FromBody] PostUrlGroupRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUrlGroupCommand(request.Name, request.Description, request.OwnerId);
        CreateUrlGroupCommandResult result = await _mediator.Send(command, cancellationToken);

        var response = new PostUrlGroupResponse(result.GroupId);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}