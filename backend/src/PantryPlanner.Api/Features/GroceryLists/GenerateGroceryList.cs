using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed partial class GroceryListsController
{
    [HttpPost("generate")]
    [ProducesResponseType<GroceryListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroceryListResponse>> Generate([FromBody] GenerateGroceryListCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record GenerateGroceryListCommand : IRequest<Result<GroceryListResponse>>
{
    public Guid UserId { get; init; }

    public Guid MealPlanId { get; init; }
}

public sealed class GenerateGroceryListCommandValidator : AbstractValidator<GenerateGroceryListCommand>
{
    public GenerateGroceryListCommandValidator()
    {
        RuleFor(command => command.MealPlanId)
            .NotEqual(Guid.Empty)
            .WithMessage("MealPlanId is required.");
    }
}

public sealed class GenerateGroceryListHandler : IRequestHandler<GenerateGroceryListCommand, Result<GroceryListResponse>>
{
    private readonly IGroceryListGenerator _generator;
    private readonly PantryPlannerDbContext _dbContext;

    public GenerateGroceryListHandler(IGroceryListGenerator generator, PantryPlannerDbContext dbContext)
    {
        _generator = generator;
        _dbContext = dbContext;
    }

    public async Task<Result<GroceryListResponse>> Handle(GenerateGroceryListCommand request, CancellationToken cancellationToken)
    {
        var groceryListResult = await _generator.GenerateAsync(request.UserId, request.MealPlanId, cancellationToken);

        if (groceryListResult.IsFailure)
        {
            return Result<GroceryListResponse>.Failure(groceryListResult.Error!);
        }

        var groceryList = groceryListResult.Value;

        await _dbContext.AddAsync(groceryList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<GroceryListResponse>.Success(request.ToResponse(groceryList));
    }
}

file static class GenerateGroceryListCommandMappings
{
    public static GroceryListResponse ToResponse(this GenerateGroceryListCommand _, GroceryList groceryList)
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
