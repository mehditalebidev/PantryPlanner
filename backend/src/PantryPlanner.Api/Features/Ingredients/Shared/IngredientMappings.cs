namespace PantryPlanner.Api.Features.Ingredients;

public static class IngredientMappings
{
    public static IngredientResponse ToResponse(this Ingredient ingredient)
    {
        return new IngredientResponse(
            ingredient.Id,
            ingredient.Name,
            ingredient.NormalizedName,
            ingredient.CreatedAt,
            ingredient.UpdatedAt);
    }
}
