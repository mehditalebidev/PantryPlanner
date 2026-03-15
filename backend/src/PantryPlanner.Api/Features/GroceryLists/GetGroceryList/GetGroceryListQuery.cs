using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed record GetGroceryListQuery(Guid UserId, Guid GroceryListId) : IRequest<Result<GroceryListResponse>>;
