using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;

namespace PantryPlanner.Api.Features.Users;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public sealed partial class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }
}
