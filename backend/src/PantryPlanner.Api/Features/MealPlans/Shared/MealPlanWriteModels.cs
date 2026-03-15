namespace PantryPlanner.Api.Features.MealPlans;

public sealed record MealSlotWriteModel
{
    public string ReferenceKey { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public bool IsDefault { get; init; }
}

public sealed record PlannedMealWriteModel
{
    public DateOnly PlannedDate { get; init; }

    public string MealSlotReferenceKey { get; init; } = string.Empty;

    public Guid RecipeId { get; init; }

    public int? ServingsOverride { get; init; }

    public string? Note { get; init; }
}
