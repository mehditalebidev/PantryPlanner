using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.RecipeImports;

public static class RecipeImportErrors
{
    public static Error NotFound(Guid recipeImportId)
    {
        return new Error(
            "recipe_import_not_found",
            "Recipe import was not found.",
            $"Recipe import '{recipeImportId}' does not exist for the current user.",
            StatusCodes.Status404NotFound);
    }
}
