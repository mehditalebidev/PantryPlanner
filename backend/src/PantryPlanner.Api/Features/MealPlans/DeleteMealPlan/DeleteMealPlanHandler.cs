using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class DeleteMealPlanHandler : IRequestHandler<DeleteMealPlanCommand, Result>
{
    private readonly IRepository _repository;

    public DeleteMealPlanHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteMealPlanCommand request, CancellationToken cancellationToken)
    {
        var mealPlan = await _repository.Query<MealPlan>()
            .SingleOrDefaultAsync(mealPlan => mealPlan.UserId == request.UserId && mealPlan.Id == request.MealPlanId, cancellationToken);

        if (mealPlan is null)
        {
            return Result.Failure(MealPlanErrors.NotFound(request.MealPlanId));
        }

        _repository.Remove(mealPlan);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
