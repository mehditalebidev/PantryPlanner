using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.RecipeImports;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/recipe-imports")]
public sealed partial class RecipeImportsController : ControllerBase
{
    private readonly ISender _sender;

    public RecipeImportsController(ISender sender)
    {
        _sender = sender;
    }
}
