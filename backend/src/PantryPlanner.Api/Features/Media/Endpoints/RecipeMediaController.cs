using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Media;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/recipes/{recipeId:guid}/media")]
public sealed partial class RecipeMediaController : ControllerBase
{
    private readonly ISender _sender;

    public RecipeMediaController(ISender sender)
    {
        _sender = sender;
    }
}
