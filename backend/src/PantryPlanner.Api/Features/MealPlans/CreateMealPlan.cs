using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed partial class MealPlansController
{
    [HttpPost]
    [ProducesResponseType<MealPlanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MealPlanResponse>> Create([FromBody] CreateMealPlanCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record CreateMealPlanCommand : IRequest<Result<MealPlanResponse>>, IMealPlanUpsertRequest
{
    public Guid UserId { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public IReadOnlyCollection<MealSlotWriteModel> Slots { get; init; } = [];

    public IReadOnlyCollection<PlannedMealWriteModel> Entries { get; init; } = [];
}

public sealed class CreateMealPlanCommandValidator : AbstractValidator<CreateMealPlanCommand>
{
    public CreateMealPlanCommandValidator()
    {
        MealPlanValidation.ApplyMealPlanRules(this);
    }
}

public sealed class CreateMealPlanHandler : IRequestHandler<CreateMealPlanCommand, Result<MealPlanResponse>>
{
    private readonly IMealPlanContentFactory _contentFactory;
    private readonly PantryPlannerDbContext _dbContext;

    public CreateMealPlanHandler(IMealPlanContentFactory contentFactory, PantryPlannerDbContext dbContext)
    {
        _contentFactory = contentFactory;
        _dbContext = dbContext;
    }

    public async Task<Result<MealPlanResponse>> Handle(CreateMealPlanCommand request, CancellationToken cancellationToken)
    {
        var contentResult = await _contentFactory.BuildAsync(request.UserId, request.Slots, request.Entries, cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<MealPlanResponse>.Failure(contentResult.Error!);
        }

        var mealPlan = MealPlan.Create(request.UserId, request.Title, request.StartDate, request.EndDate);
        mealPlan.ReplaceSlots(contentResult.Value.Slots);
        mealPlan.ReplaceEntries(contentResult.Value.Entries);

        await _dbContext.AddAsync(mealPlan, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<MealPlanResponse>.Success(request.ToResponse(mealPlan));
    }
}

file static class CreateMealPlanCommandMappings
{
    public static MealPlanResponse ToResponse(this CreateMealPlanCommand _, MealPlan mealPlan)
    {
        var orderedSlots = mealPlan.Slots
            .OrderBy(slot => slot.SortOrder)
            .ToArray();

        return new MealPlanResponse(
            mealPlan.Id,
            mealPlan.Title,
            mealPlan.StartDate,
            mealPlan.EndDate,
            orderedSlots.Select(slot => ToResponse(slot)).ToArray(),
            mealPlan.Entries
                .OrderBy(entry => entry.PlannedDate)
                .ThenBy(entry => entry.MealSlot.SortOrder)
                .Select(entry => ToResponse(entry))
                .ToArray(),
            mealPlan.CreatedAt,
            mealPlan.UpdatedAt);
    }

    private static MealSlotResponse ToResponse(MealSlot slot)
    {
        return new MealSlotResponse(
            slot.Id,
            slot.ReferenceKey,
            slot.Name,
            slot.SortOrder,
            slot.IsDefault);
    }

    private static PlannedMealResponse ToResponse(PlannedMeal entry)
    {
        return new PlannedMealResponse(
            entry.Id,
            entry.PlannedDate,
            entry.MealSlotId,
            entry.MealSlot.ReferenceKey,
            entry.RecipeId,
            entry.Recipe.Title,
            entry.ServingsOverride,
            entry.Note);
    }
}
