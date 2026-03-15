using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class PlannedMeal
{
    private PlannedMeal()
    {
    }

    private PlannedMeal(MealSlot mealSlot, Recipe recipe, DateOnly plannedDate, int? servingsOverride, string? note)
    {
        Id = Guid.NewGuid();
        MealSlot = mealSlot;
        MealSlotId = mealSlot.Id;
        Recipe = recipe;
        RecipeId = recipe.Id;
        PlannedDate = plannedDate;
        ServingsOverride = servingsOverride;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public Guid Id { get; private set; }

    public Guid MealPlanId { get; private set; }

    public MealPlan MealPlan { get; private set; } = null!;

    public Guid MealSlotId { get; private set; }

    public MealSlot MealSlot { get; private set; } = null!;

    public DateOnly PlannedDate { get; private set; }

    public Guid RecipeId { get; private set; }

    public Recipe Recipe { get; private set; } = null!;

    public int? ServingsOverride { get; private set; }

    public string? Note { get; private set; }

    public static PlannedMeal Create(MealSlot mealSlot, Recipe recipe, DateOnly plannedDate, int? servingsOverride, string? note)
    {
        return new PlannedMeal(mealSlot, recipe, plannedDate, servingsOverride, note);
    }
}
