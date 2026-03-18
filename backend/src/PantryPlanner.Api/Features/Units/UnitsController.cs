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
public sealed partial class UnitsController : ControllerBase
{
    private readonly ISender _sender;

    public UnitsController(ISender sender)
    {
        _sender = sender;
    }
}
