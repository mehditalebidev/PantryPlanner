using FluentValidation;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class UpdateIngredientCommandValidator : AbstractValidator<UpdateIngredientCommand>
{
    public UpdateIngredientCommandValidator()
    {
        RuleFor(command => command.IngredientId)
            .NotEmpty()
            .WithMessage("IngredientId is required.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must be 200 characters or fewer.");
    }
}
