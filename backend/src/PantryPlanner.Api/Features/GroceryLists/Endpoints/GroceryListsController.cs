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
public sealed class GroceryListsController : ControllerBase
{
    private readonly ISender _sender;

    public GroceryListsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("generate")]
    [ProducesResponseType<GroceryListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroceryListResponse>> Generate([FromBody] GenerateGroceryListCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GroceryListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroceryListResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGroceryListQuery(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType<GroceryListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroceryListResponse>> UpdateItem(
        Guid id,
        Guid itemId,
        [FromBody] UpdateGroceryListItemCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command with { UserId = User.GetRequiredUserId(), GroceryListId = id, GroceryListItemId = itemId },
            cancellationToken);
        return result.ToActionResult(this);
    }
}
