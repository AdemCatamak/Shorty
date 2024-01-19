using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shorty.Api.Controllers.Contracts;
using Shorty.Application;

namespace Shorty.Api.Controllers;

[ApiController]
[Route("api")]
public class UrlController : ControllerBase
{
    private readonly IMediator _mediator;

    public UrlController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("owners/{ownerId}/url-groups/{groupId}/urls")]
    public async Task<IActionResult> GetUrlsAsync([FromRoute] string ownerId, [FromRoute] Guid groupId, [FromQuery] GetUrlListRequest request, CancellationToken cancellationToken)
    {
        var command = new GetUrlListCommand(ownerId, groupId, request.PageNumber, request.PageSize);
        var result = await _mediator.Send(command, cancellationToken);

        var responseList = result.Data.Select(d => new GetUrlResponse(d.OriginalUrl, d.ShortUrl)).ToList();
        var response = new GetUrlListResponse(responseList);
        return StatusCode(StatusCodes.Status200OK, response);
    }
    
    [HttpPost("urls")]
    public async Task<IActionResult> PostUrlAsync([FromBody] PostUrlRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUrlCommand(request.OriginalUrl, request.ShortUrl, request.GroupUrlId, request.OwnerId);
        var result = await _mediator.Send(command, cancellationToken);
        
        var response = new PostUrlResponse(result.UrlId);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}