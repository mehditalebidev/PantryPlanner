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
public sealed class RecipeMediaController : ControllerBase
{
    private readonly ISender _sender;

    public RecipeMediaController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType<RecipeMediaAssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeMediaAssetResponse>> Upload(
        Guid recipeId,
        [FromForm] UploadRecipeMediaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { RecipeId = recipeId, UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{mediaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid recipeId, Guid mediaId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteRecipeMediaCommand(User.GetRequiredUserId(), recipeId, mediaId), cancellationToken);
        return result.ToActionResult(this);
    }
}
