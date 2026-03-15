using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public interface IRecipeContentFactory
{
    Task<Result<RecipeContentDraft>> BuildAsync(
        Guid userId,
        Guid? recipeId,
        IReadOnlyCollection<RecipeIngredientWriteModel> ingredientRequests,
        IReadOnlyCollection<RecipeStepWriteModel> stepRequests,
        IReadOnlyCollection<RecipeMediaAssetWriteModel> mediaRequests,
        CancellationToken cancellationToken);
}
