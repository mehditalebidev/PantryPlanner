namespace PantryPlanner.Api.Features.Recipes;

public sealed record RecipeContentDraft(
    IReadOnlyCollection<RecipeIngredient> Ingredients,
    IReadOnlyCollection<RecipeStep> Steps,
    IReadOnlyCollection<RecipeMediaAsset> MediaAssets);
