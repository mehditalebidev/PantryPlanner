using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed partial class GroceryListsController
{
    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType<GroceryListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroceryListResponse>> UpdateItem(
        Guid id,
        Guid itemId,
        [FromBody] UpdateGroceryListItemCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command with { UserId = User.GetRequiredUserId(), GroceryListId = id, GroceryListItemId = itemId },
            cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record UpdateGroceryListItemCommand : IRequest<Result<GroceryListResponse>>
{
    public Guid UserId { get; init; }

    public Guid GroceryListId { get; init; }

    public Guid GroceryListItemId { get; init; }

    public bool IsChecked { get; init; }
}

public sealed class UpdateGroceryListItemCommandValidator : AbstractValidator<UpdateGroceryListItemCommand>
{
    public UpdateGroceryListItemCommandValidator()
    {
        RuleFor(command => command.GroceryListItemId)
            .NotEqual(Guid.Empty)
            .WithMessage("GroceryListItemId is required.");
    }
}

public sealed class UpdateGroceryListItemHandler : IRequestHandler<UpdateGroceryListItemCommand, Result<GroceryListResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public UpdateGroceryListItemHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GroceryListResponse>> Handle(UpdateGroceryListItemCommand request, CancellationToken cancellationToken)
    {
        var groceryList = await _dbContext.Set<GroceryList>()
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
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<GroceryListResponse>.Success(request.ToResponse(groceryList));
    }
}

file static class UpdateGroceryListItemCommandMappings
{
    public static GroceryListResponse ToResponse(this UpdateGroceryListItemCommand _, GroceryList groceryList)
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
