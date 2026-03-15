using PantryPlanner.Api.Features.Units;

namespace PantryPlanner.Api.Features.Recipes;

public static class RecipeMappings
{
    public static RecipeResponse ToResponse(this Recipe recipe)
    {
        return new RecipeResponse(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.Servings,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.SourceUrl,
            recipe.Ingredients
                .OrderBy(ingredient => ingredient.SortOrder)
                .Select(ingredient => ingredient.ToResponse())
                .ToArray(),
            recipe.Steps
                .OrderBy(step => step.SortOrder)
                .Select(step => step.ToResponse())
                .ToArray(),
            recipe.MediaAssets
                .OrderBy(mediaAsset => mediaAsset.SortOrder)
                .Select(mediaAsset => mediaAsset.ToResponse())
                .ToArray(),
            recipe.CreatedAt,
            recipe.UpdatedAt);
    }

    private static RecipeIngredientResponse ToResponse(this RecipeIngredient ingredient)
    {
        return new RecipeIngredientResponse(
            ingredient.Id,
            ingredient.IngredientId,
            ingredient.Ingredient.Name,
            ingredient.ReferenceKey,
            ingredient.Quantity,
            ingredient.UnitCode,
            ingredient.NormalizedQuantity,
            ingredient.NormalizedUnitCode,
            ingredient.PreparationNote,
            ingredient.SortOrder);
    }

    private static RecipeStepResponse ToResponse(this RecipeStep step)
    {
        return new RecipeStepResponse(
            step.Id,
            step.Instruction,
            step.SortOrder,
            step.DurationMinutes,
            step.IngredientReferences
                .Select(reference => new RecipeStepIngredientReferenceResponse(
                    reference.RecipeIngredientId,
                    reference.RecipeIngredient.IngredientId,
                    reference.RecipeIngredient.ReferenceKey))
                .ToArray());
    }

    public static RecipeMediaAssetResponse ToResponse(this RecipeMediaAsset mediaAsset)
    {
        return new RecipeMediaAssetResponse(
            mediaAsset.Id,
            mediaAsset.Kind,
            mediaAsset.StorageKey,
            mediaAsset.Url,
            mediaAsset.ContentType,
            mediaAsset.Caption,
            mediaAsset.SortOrder);
    }
}
