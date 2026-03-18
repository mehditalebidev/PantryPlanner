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
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteMealPlanCommand(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record DeleteMealPlanCommand(Guid UserId, Guid MealPlanId) : IRequest<Result>;

public sealed class DeleteMealPlanHandler : IRequestHandler<DeleteMealPlanCommand, Result>
{
    private readonly PantryPlannerDbContext _dbContext;

    public DeleteMealPlanHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteMealPlanCommand request, CancellationToken cancellationToken)
    {
        var mealPlan = await _dbContext.Set<MealPlan>()
            .SingleOrDefaultAsync(mealPlan => mealPlan.UserId == request.UserId && mealPlan.Id == request.MealPlanId, cancellationToken);

        if (mealPlan is null)
        {
            return Result.Failure(MealPlanErrors.NotFound(request.MealPlanId));
        }

        _dbContext.Remove(mealPlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
