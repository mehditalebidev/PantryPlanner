using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed partial class GroceryListsController
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<GroceryListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroceryListResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGroceryListQuery(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record GetGroceryListQuery(Guid UserId, Guid GroceryListId) : IRequest<Result<GroceryListResponse>>;

public sealed class GetGroceryListHandler : IRequestHandler<GetGroceryListQuery, Result<GroceryListResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public GetGroceryListHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GroceryListResponse>> Handle(GetGroceryListQuery request, CancellationToken cancellationToken)
    {
        var groceryList = await _dbContext.Set<GroceryList>()
            .Where(groceryList => groceryList.UserId == request.UserId && groceryList.Id == request.GroceryListId)
            .IncludeItems()
            .SingleOrDefaultAsync(cancellationToken);

        if (groceryList is null)
        {
            return Result<GroceryListResponse>.Failure(GroceryListErrors.GroceryListNotFound(request.GroceryListId));
        }

        return Result<GroceryListResponse>.Success(request.ToResponse(groceryList));
    }
}

file static class GetGroceryListQueryMappings
{
    public static GroceryListResponse ToResponse(this GetGroceryListQuery _, GroceryList groceryList)
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
                .Select(item => ToResponse(item))
                .ToArray());
    }

    private static GroceryListItemResponse ToResponse(GroceryListItem item)
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
