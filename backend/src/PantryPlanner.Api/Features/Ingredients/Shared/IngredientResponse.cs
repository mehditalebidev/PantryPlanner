namespace PantryPlanner.Api.Features.Ingredients;

public sealed record IngredientResponse(
    Guid Id,
    string Name,
    string NormalizedName,
    DateTime CreatedAt,
    DateTime UpdatedAt);
