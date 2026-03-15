using FluentValidation;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GenerateGroceryListCommandValidator : AbstractValidator<GenerateGroceryListCommand>
{
    public GenerateGroceryListCommandValidator()
    {
        RuleFor(command => command.MealPlanId)
            .NotEqual(Guid.Empty)
            .WithMessage("MealPlanId is required.");
    }
}
