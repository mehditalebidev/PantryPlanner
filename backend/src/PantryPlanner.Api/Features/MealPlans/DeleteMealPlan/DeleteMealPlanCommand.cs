using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed record DeleteMealPlanCommand(Guid UserId, Guid MealPlanId) : IRequest<Result>;
