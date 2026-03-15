using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class GetMealPlanHandler : IRequestHandler<GetMealPlanQuery, Result<MealPlanResponse>>
{
    private readonly IRepository _repository;

    public GetMealPlanHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<MealPlanResponse>> Handle(GetMealPlanQuery request, CancellationToken cancellationToken)
    {
        var mealPlan = await _repository.Query<MealPlan>()
            .Where(mealPlan => mealPlan.UserId == request.UserId && mealPlan.Id == request.MealPlanId)
            .IncludeMealPlanDetails()
            .SingleOrDefaultAsync(cancellationToken);

        if (mealPlan is null)
        {
            return Result<MealPlanResponse>.Failure(MealPlanErrors.NotFound(request.MealPlanId));
        }

        return Result<MealPlanResponse>.Success(mealPlan.ToResponse());
    }
}
