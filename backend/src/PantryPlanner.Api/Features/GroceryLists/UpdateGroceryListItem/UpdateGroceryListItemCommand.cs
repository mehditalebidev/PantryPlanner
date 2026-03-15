using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed record UpdateGroceryListItemCommand : IRequest<Result<GroceryListResponse>>
{
    public Guid UserId { get; init; }

    public Guid GroceryListId { get; init; }

    public Guid GroceryListItemId { get; init; }

    public bool IsChecked { get; init; }
}
