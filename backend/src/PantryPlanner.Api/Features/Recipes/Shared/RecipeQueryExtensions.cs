using Microsoft.EntityFrameworkCore;

namespace PantryPlanner.Api.Features.Recipes;

internal static class RecipeQueryExtensions
{
    public static IQueryable<Recipe> IncludeRecipeDetails(this IQueryable<Recipe> query)
    {
        return query
            .Include(recipe => recipe.Ingredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .Include(recipe => recipe.Steps)
                .ThenInclude(step => step.IngredientReferences)
                    .ThenInclude(reference => reference.RecipeIngredient)
                        .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .Include(recipe => recipe.MediaAssets);
    }
}
