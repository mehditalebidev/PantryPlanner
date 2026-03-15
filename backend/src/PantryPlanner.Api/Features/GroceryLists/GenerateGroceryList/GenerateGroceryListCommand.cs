using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed record GenerateGroceryListCommand : IRequest<Result<GroceryListResponse>>
{
    public Guid UserId { get; init; }

    public Guid MealPlanId { get; init; }
}
