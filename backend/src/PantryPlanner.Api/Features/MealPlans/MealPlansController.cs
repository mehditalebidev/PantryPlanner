using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.MealPlans;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/meal-plans")]
public sealed partial class MealPlansController : ControllerBase
{
    private readonly ISender _sender;

    public MealPlansController(ISender sender)
    {
        _sender = sender;
    }
}
