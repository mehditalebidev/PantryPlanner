using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;

namespace PantryPlanner.Api.Features.Units;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/units")]
public sealed class UnitsController : ControllerBase
{
    private readonly ISender _sender;

    public UnitsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<UnitDefinitionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<UnitDefinitionResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListUnitsQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
}
