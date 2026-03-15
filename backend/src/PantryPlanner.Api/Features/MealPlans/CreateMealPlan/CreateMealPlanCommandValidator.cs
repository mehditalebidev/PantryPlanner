using FluentValidation;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class CreateMealPlanCommandValidator : AbstractValidator<CreateMealPlanCommand>
{
    public CreateMealPlanCommandValidator()
    {
        MealPlanValidation.ApplyMealPlanRules(this);
    }
}
