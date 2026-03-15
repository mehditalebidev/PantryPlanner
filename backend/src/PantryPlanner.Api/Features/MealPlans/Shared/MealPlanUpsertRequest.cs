namespace PantryPlanner.Api.Features.MealPlans;

public interface IMealPlanUpsertRequest
{
    string Title { get; }

    DateOnly StartDate { get; }

    DateOnly EndDate { get; }

    IReadOnlyCollection<MealSlotWriteModel> Slots { get; }

    IReadOnlyCollection<PlannedMealWriteModel> Entries { get; }
}
