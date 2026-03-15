using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public interface IGroceryListGenerator
{
    Task<Result<GroceryList>> GenerateAsync(Guid userId, Guid mealPlanId, CancellationToken cancellationToken);
}
