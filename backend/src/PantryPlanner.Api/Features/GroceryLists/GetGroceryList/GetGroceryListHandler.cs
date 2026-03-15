using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GetGroceryListHandler : IRequestHandler<GetGroceryListQuery, Result<GroceryListResponse>>
{
    private readonly IRepository _repository;

    public GetGroceryListHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GroceryListResponse>> Handle(GetGroceryListQuery request, CancellationToken cancellationToken)
    {
        var groceryList = await _repository.Query<GroceryList>()
            .Where(groceryList => groceryList.UserId == request.UserId && groceryList.Id == request.GroceryListId)
            .IncludeItems()
            .SingleOrDefaultAsync(cancellationToken);

        if (groceryList is null)
        {
            return Result<GroceryListResponse>.Failure(GroceryListErrors.GroceryListNotFound(request.GroceryListId));
        }

        return Result<GroceryListResponse>.Success(groceryList.ToResponse());
    }
}
