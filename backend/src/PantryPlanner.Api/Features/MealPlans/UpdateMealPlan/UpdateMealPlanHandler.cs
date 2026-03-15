using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class UpdateMealPlanHandler : IRequestHandler<UpdateMealPlanCommand, Result<MealPlanResponse>>
{
    private readonly IMealPlanContentFactory _contentFactory;
    private readonly IRepository _repository;

    public UpdateMealPlanHandler(IMealPlanContentFactory contentFactory, IRepository repository)
    {
        _contentFactory = contentFactory;
        _repository = repository;
    }

    public async Task<Result<MealPlanResponse>> Handle(UpdateMealPlanCommand request, CancellationToken cancellationToken)
    {
        var mealPlan = await _repository.Query<MealPlan>()
            .SingleOrDefaultAsync(mealPlan => mealPlan.UserId == request.UserId && mealPlan.Id == request.MealPlanId, cancellationToken);

        if (mealPlan is null)
        {
            return Result<MealPlanResponse>.Failure(MealPlanErrors.NotFound(request.MealPlanId));
        }

        var contentResult = await _contentFactory.BuildAsync(request.UserId, request.Slots, request.Entries, cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<MealPlanResponse>.Failure(contentResult.Error!);
        }

        mealPlan.UpdateDetails(request.Title, request.StartDate, request.EndDate);

        await _repository.DbContext.PlannedMeals
            .Where(entry => entry.MealPlanId == request.MealPlanId)
            .ExecuteDeleteAsync(cancellationToken);

        await _repository.DbContext.MealSlots
            .Where(slot => slot.MealPlanId == request.MealPlanId)
            .ExecuteDeleteAsync(cancellationToken);

        mealPlan.ReplaceSlots(contentResult.Value.Slots);
        mealPlan.ReplaceEntries(contentResult.Value.Entries);

        foreach (var slot in mealPlan.Slots)
        {
            _repository.DbContext.Entry(slot).State = EntityState.Added;
        }

        foreach (var entry in mealPlan.Entries)
        {
            _repository.DbContext.Entry(entry).State = EntityState.Added;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result<MealPlanResponse>.Success(mealPlan.ToResponse());
    }
}
