using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Media;
using PantryPlanner.Api.Features.MealPlans;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class DeleteRecipeHandler : IRequestHandler<DeleteRecipeCommand, Result>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly IRepository _repository;

    public DeleteRecipeHandler(IRepository repository, IMediaStorage mediaStorage)
    {
        _repository = repository;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _repository.Query<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        var isUsedByMealPlan = await _repository.Query<PlannedMeal>()
            .AnyAsync(plannedMeal => plannedMeal.RecipeId == request.RecipeId, cancellationToken);

        if (isUsedByMealPlan)
        {
            return Result.Failure(RecipeErrors.InUseByMealPlan());
        }

        var mediaStorageKeys = await _repository.DbContext.RecipeMediaAssets
            .Where(mediaAsset => mediaAsset.RecipeId == request.RecipeId && mediaAsset.StorageKey != null)
            .Select(mediaAsset => mediaAsset.StorageKey!)
            .ToArrayAsync(cancellationToken);

        _repository.Remove(recipe);
        await _repository.SaveChangesAsync(cancellationToken);

        foreach (var storageKey in mediaStorageKeys)
        {
            await _mediaStorage.DeleteIfExistsAsync(storageKey, cancellationToken);
        }

        return Result.Success();
    }
}
