namespace PantryPlanner.Api.Features.MealPlans;

public static class MealPlanMappings
{
    public static MealPlanResponse ToResponse(this MealPlan mealPlan)
    {
        var orderedSlots = mealPlan.Slots
            .OrderBy(slot => slot.SortOrder)
            .ToArray();

        return new MealPlanResponse(
            mealPlan.Id,
            mealPlan.Title,
            mealPlan.StartDate,
            mealPlan.EndDate,
            orderedSlots.Select(slot => slot.ToResponse()).ToArray(),
            mealPlan.Entries
                .OrderBy(entry => entry.PlannedDate)
                .ThenBy(entry => entry.MealSlot.SortOrder)
                .Select(entry => entry.ToResponse())
                .ToArray(),
            mealPlan.CreatedAt,
            mealPlan.UpdatedAt);
    }

    private static MealSlotResponse ToResponse(this MealSlot slot)
    {
        return new MealSlotResponse(
            slot.Id,
            slot.ReferenceKey,
            slot.Name,
            slot.SortOrder,
            slot.IsDefault);
    }

    private static PlannedMealResponse ToResponse(this PlannedMeal entry)
    {
        return new PlannedMealResponse(
            entry.Id,
            entry.PlannedDate,
            entry.MealSlotId,
            entry.MealSlot.ReferenceKey,
            entry.RecipeId,
            entry.Recipe.Title,
            entry.ServingsOverride,
            entry.Note);
    }
}
