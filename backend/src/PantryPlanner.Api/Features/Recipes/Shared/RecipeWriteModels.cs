namespace PantryPlanner.Api.Features.Recipes;

public sealed record RecipeIngredientWriteModel
{
    public Guid? IngredientId { get; init; }

    public string? Name { get; init; }

    public string ReferenceKey { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public string UnitCode { get; init; } = string.Empty;

    public string? PreparationNote { get; init; }

    public int SortOrder { get; init; }
}

public sealed record RecipeStepWriteModel
{
    public string Instruction { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public int? DurationMinutes { get; init; }

    public IReadOnlyCollection<string> IngredientReferenceKeys { get; init; } = [];
}

public sealed record RecipeMediaAssetWriteModel
{
    public string Kind { get; init; } = string.Empty;

    public string? StorageKey { get; init; }

    public string Url { get; init; } = string.Empty;

    public string? Caption { get; init; }

    public int SortOrder { get; init; }
}
