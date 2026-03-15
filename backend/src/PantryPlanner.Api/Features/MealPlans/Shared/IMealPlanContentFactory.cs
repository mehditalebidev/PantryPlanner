using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public interface IMealPlanContentFactory
{
    Task<Result<MealPlanContentDraft>> BuildAsync(
        Guid userId,
        IReadOnlyCollection<MealSlotWriteModel> slots,
        IReadOnlyCollection<PlannedMealWriteModel> entries,
        CancellationToken cancellationToken);
}
