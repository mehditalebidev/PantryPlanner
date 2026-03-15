namespace PantryPlanner.Api.Features.MealPlans;

public sealed record MealPlanContentDraft(
    IReadOnlyCollection<MealSlot> Slots,
    IReadOnlyCollection<PlannedMeal> Entries);
