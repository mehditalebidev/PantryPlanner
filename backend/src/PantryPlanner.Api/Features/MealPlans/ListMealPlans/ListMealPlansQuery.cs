using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed record ListMealPlansQuery(Guid UserId) : IRequest<Result<IReadOnlyCollection<MealPlanResponse>>>;
