using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class ListMealPlansHandler : IRequestHandler<ListMealPlansQuery, Result<IReadOnlyCollection<MealPlanResponse>>>
{
    private readonly IRepository _repository;

    public ListMealPlansHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyCollection<MealPlanResponse>>> Handle(ListMealPlansQuery request, CancellationToken cancellationToken)
    {
        var mealPlans = await _repository.Query<MealPlan>()
            .Where(mealPlan => mealPlan.UserId == request.UserId)
            .IncludeMealPlanDetails()
            .OrderByDescending(mealPlan => mealPlan.StartDate)
            .ThenByDescending(mealPlan => mealPlan.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<MealPlanResponse>>.Success(mealPlans.Select(mealPlan => mealPlan.ToResponse()).ToArray());
    }
}
