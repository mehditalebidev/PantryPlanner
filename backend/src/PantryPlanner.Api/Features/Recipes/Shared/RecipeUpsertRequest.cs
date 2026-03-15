namespace PantryPlanner.Api.Features.Recipes;

public interface IRecipeUpsertRequest
{
    string Title { get; }

    string? Description { get; }

    int Servings { get; }

    int? PrepTimeMinutes { get; }

    int? CookTimeMinutes { get; }

    string? SourceUrl { get; }

    IReadOnlyCollection<RecipeIngredientWriteModel> Ingredients { get; }

    IReadOnlyCollection<RecipeStepWriteModel> Steps { get; }

    IReadOnlyCollection<RecipeMediaAssetWriteModel> Media { get; }
}
