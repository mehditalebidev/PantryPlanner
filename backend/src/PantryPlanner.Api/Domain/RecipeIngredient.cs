using PantryPlanner.Api.Features.Ingredients;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeIngredient
{
    private RecipeIngredient()
    {
    }

    private RecipeIngredient(
        Ingredient ingredient,
        string referenceKey,
        decimal quantity,
        string unitCode,
        decimal? normalizedQuantity,
        string? normalizedUnitCode,
        string? preparationNote,
        int sortOrder)
    {
        Id = Guid.NewGuid();
        Ingredient = ingredient;
        IngredientId = ingredient.Id;
        ReferenceKey = referenceKey.Trim();
        Quantity = quantity;
        UnitCode = unitCode.Trim();
        NormalizedQuantity = normalizedQuantity;
        NormalizedUnitCode = string.IsNullOrWhiteSpace(normalizedUnitCode) ? null : normalizedUnitCode.Trim();
        PreparationNote = string.IsNullOrWhiteSpace(preparationNote) ? null : preparationNote.Trim();
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }

    public Guid RecipeId { get; private set; }

    public Recipe Recipe { get; private set; } = null!;

    public Guid IngredientId { get; private set; }

    public Ingredient Ingredient { get; private set; } = null!;

    public string ReferenceKey { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string UnitCode { get; private set; } = string.Empty;

    public decimal? NormalizedQuantity { get; private set; }

    public string? NormalizedUnitCode { get; private set; }

    public string? PreparationNote { get; private set; }

    public int SortOrder { get; private set; }

    public static RecipeIngredient Create(
        Ingredient ingredient,
        string referenceKey,
        decimal quantity,
        string unitCode,
        decimal? normalizedQuantity,
        string? normalizedUnitCode,
        string? preparationNote,
        int sortOrder)
    {
        return new RecipeIngredient(
            ingredient,
            referenceKey,
            quantity,
            unitCode,
            normalizedQuantity,
            normalizedUnitCode,
            preparationNote,
            sortOrder);
    }
}
