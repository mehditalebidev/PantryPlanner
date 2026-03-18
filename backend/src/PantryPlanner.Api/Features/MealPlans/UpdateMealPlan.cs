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

namespace PantryPlanner.Api.Features.MealPlans;

public sealed partial class MealPlansController
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType<MealPlanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MealPlanResponse>> Update(
        Guid id,
        [FromBody] UpdateMealPlanCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { MealPlanId = id, UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record UpdateMealPlanCommand : IRequest<Result<MealPlanResponse>>, IMealPlanUpsertRequest
{
    public Guid UserId { get; init; }

    public Guid MealPlanId { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public IReadOnlyCollection<MealSlotWriteModel> Slots { get; init; } = [];

    public IReadOnlyCollection<PlannedMealWriteModel> Entries { get; init; } = [];
}

public sealed class UpdateMealPlanCommandValidator : AbstractValidator<UpdateMealPlanCommand>
{
    public UpdateMealPlanCommandValidator()
    {
        MealPlanValidation.ApplyMealPlanRules(this);
    }
}

public sealed class UpdateMealPlanHandler : IRequestHandler<UpdateMealPlanCommand, Result<MealPlanResponse>>
{
    private readonly IMealPlanContentFactory _contentFactory;
    private readonly PantryPlannerDbContext _dbContext;

    public UpdateMealPlanHandler(IMealPlanContentFactory contentFactory, PantryPlannerDbContext dbContext)
    {
        _contentFactory = contentFactory;
        _dbContext = dbContext;
    }

    public async Task<Result<MealPlanResponse>> Handle(UpdateMealPlanCommand request, CancellationToken cancellationToken)
    {
        var mealPlan = await _dbContext.Set<MealPlan>()
            .SingleOrDefaultAsync(mealPlan => mealPlan.UserId == request.UserId && mealPlan.Id == request.MealPlanId, cancellationToken);

        if (mealPlan is null)
        {
            return Result<MealPlanResponse>.Failure(MealPlanErrors.NotFound(request.MealPlanId));
        }

        var contentResult = await _contentFactory.BuildAsync(request.UserId, request.Slots, request.Entries, cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<MealPlanResponse>.Failure(contentResult.Error!);
        }

        mealPlan.UpdateDetails(request.Title, request.StartDate, request.EndDate);

        await _dbContext.PlannedMeals
            .Where(entry => entry.MealPlanId == request.MealPlanId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.MealSlots
            .Where(slot => slot.MealPlanId == request.MealPlanId)
            .ExecuteDeleteAsync(cancellationToken);

        mealPlan.ReplaceSlots(contentResult.Value.Slots);
        mealPlan.ReplaceEntries(contentResult.Value.Entries);

        foreach (var slot in mealPlan.Slots)
        {
            _dbContext.Entry(slot).State = EntityState.Added;
        }

        foreach (var entry in mealPlan.Entries)
        {
            _dbContext.Entry(entry).State = EntityState.Added;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<MealPlanResponse>.Success(request.ToResponse(mealPlan));
    }
}

file static class UpdateMealPlanCommandMappings
{
    public static MealPlanResponse ToResponse(this UpdateMealPlanCommand _, MealPlan mealPlan)
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
