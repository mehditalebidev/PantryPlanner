using MediatR;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GenerateGroceryListHandler : IRequestHandler<GenerateGroceryListCommand, Result<GroceryListResponse>>
{
    private readonly IGroceryListGenerator _generator;
    private readonly IRepository _repository;

    public GenerateGroceryListHandler(IGroceryListGenerator generator, IRepository repository)
    {
        _generator = generator;
        _repository = repository;
    }

    public async Task<Result<GroceryListResponse>> Handle(GenerateGroceryListCommand request, CancellationToken cancellationToken)
    {
        var groceryListResult = await _generator.GenerateAsync(request.UserId, request.MealPlanId, cancellationToken);

        if (groceryListResult.IsFailure)
        {
            return Result<GroceryListResponse>.Failure(groceryListResult.Error!);
        }

        var groceryList = groceryListResult.Value;

        await _repository.AddAsync(groceryList, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<GroceryListResponse>.Success(groceryList.ToResponse());
    }
}
