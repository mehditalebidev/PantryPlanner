using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.Media;

public sealed partial class RecipeMediaController
{
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

public sealed record DeleteRecipeMediaCommand(Guid UserId, Guid RecipeId, Guid MediaId) : IRequest<Result>;

public sealed class DeleteRecipeMediaHandler : IRequestHandler<DeleteRecipeMediaCommand, Result>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly PantryPlannerDbContext _dbContext;

    public DeleteRecipeMediaHandler(PantryPlannerDbContext dbContext, IMediaStorage mediaStorage)
    {
        _dbContext = dbContext;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result> Handle(DeleteRecipeMediaCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Set<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.Id == request.RecipeId && recipe.UserId == request.UserId, cancellationToken);

        if (recipe is null)
        {
            return Result.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        var mediaAsset = await _dbContext.Set<RecipeMediaAsset>()
            .SingleOrDefaultAsync(mediaAsset => mediaAsset.Id == request.MediaId && mediaAsset.RecipeId == request.RecipeId, cancellationToken);

        if (mediaAsset is null)
        {
            return Result.Failure(MediaErrors.NotFound(request.MediaId));
        }

        var storageKey = mediaAsset.StorageKey;

        _dbContext.Remove(mediaAsset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(storageKey))
        {
            await _mediaStorage.DeleteIfExistsAsync(storageKey, cancellationToken);
        }

        return Result.Success();
    }
}
