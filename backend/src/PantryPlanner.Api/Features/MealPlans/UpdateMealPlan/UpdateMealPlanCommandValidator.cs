using FluentValidation;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class UpdateMealPlanCommandValidator : AbstractValidator<UpdateMealPlanCommand>
{
    public UpdateMealPlanCommandValidator()
    {
        MealPlanValidation.ApplyMealPlanRules(this);
    }
}
