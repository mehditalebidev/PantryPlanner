namespace PantryPlanner.Api.Features.MealPlans;

public sealed record MealPlanResponse(
    Guid Id,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<MealSlotResponse> Slots,
    IReadOnlyCollection<PlannedMealResponse> Entries,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record MealSlotResponse(
    Guid Id,
    string ReferenceKey,
    string Name,
    int SortOrder,
    bool IsDefault);

public sealed record PlannedMealResponse(
    Guid Id,
    DateOnly PlannedDate,
    Guid MealSlotId,
    string MealSlotReferenceKey,
    Guid RecipeId,
    string RecipeTitle,
    int? ServingsOverride,
    string? Note);
