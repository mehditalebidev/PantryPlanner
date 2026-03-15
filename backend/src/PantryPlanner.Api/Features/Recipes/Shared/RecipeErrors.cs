using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public static class RecipeErrors
{
    public static Error NotFound(Guid recipeId)
    {
        return new Error(
            "recipe_not_found",
            "Recipe was not found.",
            $"Recipe '{recipeId}' does not exist for the current user.",
            StatusCodes.Status404NotFound);
    }

    public static Error InvalidIngredientReference()
    {
        return new Error(
            "invalid_ingredient_reference",
            "One or more ingredients are invalid.",
            "The request referenced an ingredient that does not exist for the current user.",
            StatusCodes.Status400BadRequest);
    }

    public static Error InvalidMediaReference()
    {
        return new Error(
            "invalid_media_reference",
            "One or more media assets are invalid.",
            "The request referenced a media asset that does not exist for the current user.",
            StatusCodes.Status400BadRequest);
    }

    public static Error InUseByMealPlan()
    {
        return new Error(
            "recipe_in_use_by_meal_plan",
            "Recipe is still used by meal plans.",
            "Remove the recipe from all meal plans before deleting it.",
            StatusCodes.Status409Conflict);
    }
}
