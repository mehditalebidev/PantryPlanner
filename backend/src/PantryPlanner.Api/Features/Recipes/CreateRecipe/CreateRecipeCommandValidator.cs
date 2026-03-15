using FluentValidation;
using PantryPlanner.Api.Features.Units;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator(IUnitCatalog unitCatalog)
    {
        RecipeValidation.ApplyRecipeRules(this, unitCatalog);
    }
}
