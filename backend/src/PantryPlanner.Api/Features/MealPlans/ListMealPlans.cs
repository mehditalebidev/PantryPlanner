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
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<MealPlanResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<MealPlanResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListMealPlansQuery(User.GetRequiredUserId()), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record ListMealPlansQuery(Guid UserId) : IRequest<Result<IReadOnlyCollection<MealPlanResponse>>>;

public sealed class ListMealPlansHandler : IRequestHandler<ListMealPlansQuery, Result<IReadOnlyCollection<MealPlanResponse>>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public ListMealPlansHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<MealPlanResponse>>> Handle(ListMealPlansQuery request, CancellationToken cancellationToken)
    {
        var mealPlans = await _dbContext.Set<MealPlan>()
            .Where(mealPlan => mealPlan.UserId == request.UserId)
            .IncludeMealPlanDetails()
            .OrderByDescending(mealPlan => mealPlan.StartDate)
            .ThenByDescending(mealPlan => mealPlan.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<MealPlanResponse>>.Success(request.ToResponse(mealPlans));
    }
}

file static class ListMealPlansQueryMappings
{
    public static IReadOnlyCollection<MealPlanResponse> ToResponse(this ListMealPlansQuery _, IEnumerable<MealPlan> mealPlans)
    {
        return mealPlans.Select(mealPlan => _.ToResponse(mealPlan)).ToArray();
    }

    public static MealPlanResponse ToResponse(this ListMealPlansQuery _, MealPlan mealPlan)
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
