using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public static class MealPlanErrors
{
    public static Error NotFound(Guid mealPlanId)
    {
        return new Error(
            "meal_plan_not_found",
            "Meal plan was not found.",
            $"Meal plan '{mealPlanId}' does not exist for the current user.",
            StatusCodes.Status404NotFound);
    }

    public static Error InvalidRecipeReference()
    {
        return new Error(
            "invalid_recipe_reference",
            "One or more recipes are invalid.",
            "The request referenced a recipe that does not exist for the current user.",
            StatusCodes.Status400BadRequest);
    }

    public static Error InvalidSlotReference()
    {
        return new Error(
            "invalid_meal_slot_reference",
            "One or more meal slots are invalid.",
            "The request referenced a meal slot that does not exist in the current payload.",
            StatusCodes.Status400BadRequest);
    }
}
