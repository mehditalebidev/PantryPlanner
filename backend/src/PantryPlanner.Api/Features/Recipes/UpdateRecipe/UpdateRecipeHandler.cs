using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class UpdateRecipeHandler : IRequestHandler<UpdateRecipeCommand, Result<RecipeResponse>>
{
    private readonly IRecipeContentFactory _recipeContentFactory;
    private readonly IRepository _repository;

    public UpdateRecipeHandler(IRepository repository, IRecipeContentFactory recipeContentFactory)
    {
        _repository = repository;
        _recipeContentFactory = recipeContentFactory;
    }

    public async Task<Result<RecipeResponse>> Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _repository.Query<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result<RecipeResponse>.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        var contentResult = await _recipeContentFactory.BuildAsync(
            request.UserId,
            request.Ingredients,
            request.Steps,
            request.Media,
            cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<RecipeResponse>.Failure(contentResult.Error!);
        }

        recipe.UpdateDetails(
            request.Title,
            request.Description,
            request.Servings,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.SourceUrl);

        var existingStepIds = await _repository.DbContext.RecipeSteps
            .Where(step => step.RecipeId == request.RecipeId)
            .Select(step => step.Id)
            .ToArrayAsync(cancellationToken);

        if (existingStepIds.Length > 0)
        {
            await _repository.DbContext.RecipeStepIngredientReferences
                .Where(reference => existingStepIds.Contains(reference.RecipeStepId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await _repository.DbContext.RecipeMediaAssets
            .Where(mediaAsset => mediaAsset.RecipeId == request.RecipeId)
            .ExecuteDeleteAsync(cancellationToken);

        await _repository.DbContext.RecipeSteps
            .Where(step => step.RecipeId == request.RecipeId)
            .ExecuteDeleteAsync(cancellationToken);

        await _repository.DbContext.RecipeIngredients
            .Where(ingredient => ingredient.RecipeId == request.RecipeId)
            .ExecuteDeleteAsync(cancellationToken);

        recipe.ReplaceIngredients(contentResult.Value.Ingredients);
        recipe.ReplaceSteps(contentResult.Value.Steps);
        recipe.ReplaceMediaAssets(contentResult.Value.MediaAssets);

        foreach (var ingredient in contentResult.Value.Ingredients)
        {
            _repository.DbContext.Entry(ingredient).State = EntityState.Added;
        }

        foreach (var step in contentResult.Value.Steps)
        {
            _repository.DbContext.Entry(step).State = EntityState.Added;

            foreach (var ingredientReference in step.IngredientReferences)
            {
                _repository.DbContext.Entry(ingredientReference).State = EntityState.Added;
            }
        }

        foreach (var mediaAsset in contentResult.Value.MediaAssets)
        {
            _repository.DbContext.Entry(mediaAsset).State = EntityState.Added;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result<RecipeResponse>.Success(recipe.ToResponse());
    }
}
