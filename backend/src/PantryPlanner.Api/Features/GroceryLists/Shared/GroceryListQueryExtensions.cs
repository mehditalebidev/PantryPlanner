using Microsoft.EntityFrameworkCore;

namespace PantryPlanner.Api.Features.GroceryLists;

public static class GroceryListQueryExtensions
{
    public static IQueryable<GroceryList> IncludeItems(this IQueryable<GroceryList> query)
    {
        return query.Include(groceryList => groceryList.Items);
    }
}
