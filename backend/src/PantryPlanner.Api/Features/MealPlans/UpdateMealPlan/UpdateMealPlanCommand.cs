using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

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
