using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed partial class MealPlansController
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<MealPlanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MealPlanResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMealPlanQuery(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record GetMealPlanQuery(Guid UserId, Guid MealPlanId) : IRequest<Result<MealPlanResponse>>;

public sealed class GetMealPlanHandler : IRequestHandler<GetMealPlanQuery, Result<MealPlanResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public GetMealPlanHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<MealPlanResponse>> Handle(GetMealPlanQuery request, CancellationToken cancellationToken)
    {
        var mealPlan = await _dbContext.Set<MealPlan>()
            .Where(mealPlan => mealPlan.UserId == request.UserId && mealPlan.Id == request.MealPlanId)
            .IncludeMealPlanDetails()
            .SingleOrDefaultAsync(cancellationToken);

        if (mealPlan is null)
        {
            return Result<MealPlanResponse>.Failure(MealPlanErrors.NotFound(request.MealPlanId));
        }

        return Result<MealPlanResponse>.Success(request.ToResponse(mealPlan));
    }
}

file static class GetMealPlanQueryMappings
{
    public static MealPlanResponse ToResponse(this GetMealPlanQuery _, MealPlan mealPlan)
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
