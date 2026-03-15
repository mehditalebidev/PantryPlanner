using FluentValidation;
using PantryPlanner.Api.Features.Units;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class UpdateRecipeCommandValidator : AbstractValidator<UpdateRecipeCommand>
{
    public UpdateRecipeCommandValidator(IUnitCatalog unitCatalog)
    {
        RuleFor(command => command.RecipeId)
            .NotEmpty()
            .WithMessage("RecipeId is required.");

        RecipeValidation.ApplyRecipeRules(this, unitCatalog);
    }
}
