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
public sealed partial class MediaController : ControllerBase
{
    private readonly ISender _sender;

    public MediaController(ISender sender)
    {
        _sender = sender;
    }
}
