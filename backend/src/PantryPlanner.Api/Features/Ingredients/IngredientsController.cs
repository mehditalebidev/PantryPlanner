using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.Ingredients;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/ingredients")]
public sealed partial class IngredientsController : ControllerBase
{
    private readonly ISender _sender;

    public IngredientsController(ISender sender)
    {
        _sender = sender;
    }
}
