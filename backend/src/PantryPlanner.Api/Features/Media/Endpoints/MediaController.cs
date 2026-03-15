using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.Media;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/media")]
public sealed class MediaController : ControllerBase
{
    private readonly ISender _sender;

    public MediaController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{**storageKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string storageKey, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMediaContentQuery(User.GetRequiredUserId(), storageKey), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result.Error!);
        }

        return File(result.Value.Content, result.Value.ContentType);
    }
}
