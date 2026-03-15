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
public sealed class RecipesController : ControllerBase
{
    private readonly ISender _sender;

    public RecipesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<RecipeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<RecipeResponse>>> List(CancellationToken cancellationToken)
    {
        var query = new ListRecipesQuery(User.GetRequiredUserId());
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecipeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRecipeQuery(User.GetRequiredUserId(), id);
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType<RecipeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecipeResponse>> Create([FromBody] CreateRecipeCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<RecipeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeResponse>> Update(
        Guid id,
        [FromBody] UpdateRecipeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { RecipeId = id, UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteRecipeCommand(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }
}
