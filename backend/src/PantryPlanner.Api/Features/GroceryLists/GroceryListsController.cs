using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.GroceryLists;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/grocery-lists")]
public sealed partial class GroceryListsController : ControllerBase
{
    private readonly ISender _sender;

    public GroceryListsController(ISender sender)
    {
        _sender = sender;
    }
}
