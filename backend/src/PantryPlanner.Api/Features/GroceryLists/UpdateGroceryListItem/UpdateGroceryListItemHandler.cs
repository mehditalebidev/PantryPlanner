using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class UpdateGroceryListItemHandler : IRequestHandler<UpdateGroceryListItemCommand, Result<GroceryListResponse>>
{
    private readonly IRepository _repository;

    public UpdateGroceryListItemHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GroceryListResponse>> Handle(UpdateGroceryListItemCommand request, CancellationToken cancellationToken)
    {
        var groceryList = await _repository.Query<GroceryList>()
            .Where(groceryList => groceryList.UserId == request.UserId && groceryList.Id == request.GroceryListId)
            .IncludeItems()
            .SingleOrDefaultAsync(cancellationToken);

        if (groceryList is null)
        {
            return Result<GroceryListResponse>.Failure(GroceryListErrors.GroceryListNotFound(request.GroceryListId));
        }

        var item = groceryList.Items.SingleOrDefault(item => item.Id == request.GroceryListItemId);

        if (item is null)
        {
            return Result<GroceryListResponse>.Failure(GroceryListErrors.GroceryListItemNotFound(request.GroceryListItemId));
        }

        item.SetChecked(request.IsChecked);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<GroceryListResponse>.Success(groceryList.ToResponse());
    }
}
