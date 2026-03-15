using MediatR;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class CreateMealPlanHandler : IRequestHandler<CreateMealPlanCommand, Result<MealPlanResponse>>
{
    private readonly IMealPlanContentFactory _contentFactory;
    private readonly IRepository _repository;

    public CreateMealPlanHandler(IMealPlanContentFactory contentFactory, IRepository repository)
    {
        _contentFactory = contentFactory;
        _repository = repository;
    }

    public async Task<Result<MealPlanResponse>> Handle(CreateMealPlanCommand request, CancellationToken cancellationToken)
    {
        var contentResult = await _contentFactory.BuildAsync(request.UserId, request.Slots, request.Entries, cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<MealPlanResponse>.Failure(contentResult.Error!);
        }

        var mealPlan = MealPlan.Create(request.UserId, request.Title, request.StartDate, request.EndDate);
        mealPlan.ReplaceSlots(contentResult.Value.Slots);
        mealPlan.ReplaceEntries(contentResult.Value.Entries);

        await _repository.AddAsync(mealPlan, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<MealPlanResponse>.Success(mealPlan.ToResponse());
    }
}
