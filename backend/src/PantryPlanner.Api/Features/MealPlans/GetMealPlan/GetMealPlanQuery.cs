using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed record GetMealPlanQuery(Guid UserId, Guid MealPlanId) : IRequest<Result<MealPlanResponse>>;
