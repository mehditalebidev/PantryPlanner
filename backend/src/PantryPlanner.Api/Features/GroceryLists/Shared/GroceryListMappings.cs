namespace PantryPlanner.Api.Features.GroceryLists;

public static class GroceryListMappings
{
    public static GroceryListResponse ToResponse(this GroceryList groceryList)
    {
        return new GroceryListResponse(
            groceryList.Id,
            groceryList.MealPlanId,
            groceryList.StartDate,
            groceryList.EndDate,
            groceryList.GeneratedAt,
            groceryList.Items
                .OrderBy(item => item.Name)
                .ThenBy(item => item.UnitCode)
                .Select(item => item.ToResponse())
                .ToArray());
    }

    private static GroceryListItemResponse ToResponse(this GroceryListItem item)
    {
        return new GroceryListItemResponse(
            item.Id,
            item.IngredientId,
            item.Name,
            item.Quantity,
            item.UnitCode,
            item.IsChecked,
            item.SourceCount);
    }
}
