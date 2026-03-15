using FluentValidation;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class CreateIngredientCommandValidator : AbstractValidator<CreateIngredientCommand>
{
    public CreateIngredientCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must be 200 characters or fewer.");
    }
}
