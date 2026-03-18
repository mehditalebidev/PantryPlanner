using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Media;
using PantryPlanner.Api.Features.MealPlans;

namespace PantryPlanner.Api.Features.Recipes;

public sealed partial class RecipesController
{
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

public sealed record DeleteRecipeCommand(Guid UserId, Guid RecipeId) : IRequest<Result>;

public sealed class DeleteRecipeHandler : IRequestHandler<DeleteRecipeCommand, Result>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly PantryPlannerDbContext _dbContext;

    public DeleteRecipeHandler(PantryPlannerDbContext dbContext, IMediaStorage mediaStorage)
    {
        _dbContext = dbContext;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Set<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        var isUsedByMealPlan = await _dbContext.Set<PlannedMeal>()
            .AnyAsync(plannedMeal => plannedMeal.RecipeId == request.RecipeId, cancellationToken);

        if (isUsedByMealPlan)
        {
            return Result.Failure(RecipeErrors.InUseByMealPlan());
        }

        var mediaStorageKeys = await _dbContext.RecipeMediaAssets
            .Where(mediaAsset => mediaAsset.RecipeId == request.RecipeId && mediaAsset.StorageKey != null)
            .Select(mediaAsset => mediaAsset.StorageKey!)
            .ToArrayAsync(cancellationToken);

        _dbContext.Remove(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var storageKey in mediaStorageKeys)
        {
            await _mediaStorage.DeleteIfExistsAsync(storageKey, cancellationToken);
        }

        return Result.Success();
    }
}
