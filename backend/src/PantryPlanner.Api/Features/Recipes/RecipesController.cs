using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.Recipes;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/recipes")]
public sealed partial class RecipesController : ControllerBase
{
    private readonly ISender _sender;

    public RecipesController(ISender sender)
    {
        _sender = sender;
    }
}
