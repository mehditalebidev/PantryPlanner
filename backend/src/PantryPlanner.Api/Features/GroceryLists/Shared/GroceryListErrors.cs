using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public static class GroceryListErrors
{
    public static Error MealPlanNotFound(Guid mealPlanId)
    {
        return new Error(
            "meal_plan_not_found",
            "Meal plan was not found.",
            $"Meal plan '{mealPlanId}' does not exist for the current user.",
            StatusCodes.Status404NotFound);
    }

    public static Error GroceryListNotFound(Guid groceryListId)
    {
        return new Error(
            "grocery_list_not_found",
            "Grocery list was not found.",
            $"Grocery list '{groceryListId}' does not exist for the current user.",
            StatusCodes.Status404NotFound);
    }

    public static Error GroceryListItemNotFound(Guid groceryListItemId)
    {
        return new Error(
            "grocery_list_item_not_found",
            "Grocery list item was not found.",
            $"Grocery list item '{groceryListItemId}' does not exist for the current grocery list.",
            StatusCodes.Status404NotFound);
    }
}
