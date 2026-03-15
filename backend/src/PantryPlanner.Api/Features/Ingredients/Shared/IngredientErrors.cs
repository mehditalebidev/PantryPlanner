using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public static class IngredientErrors
{
    public static Error NotFound(Guid ingredientId)
    {
        return new Error(
            "ingredient_not_found",
            "Ingredient was not found.",
            $"The ingredient with id '{ingredientId}' was not found for the current user.",
            StatusCodes.Status404NotFound);
    }

    public static Error NameAlreadyExists()
    {
        return new Error(
            "ingredient_name_in_use",
            "Ingredient name is already in use.",
            "An ingredient with the same name already exists for the current user.",
            StatusCodes.Status409Conflict);
    }

    public static Error InUseByRecipe()
    {
        return new Error(
            "ingredient_in_use_by_recipe",
            "Ingredient is still used by recipes.",
            "Remove the ingredient from all recipes before deleting it.",
            StatusCodes.Status409Conflict);
    }
}
