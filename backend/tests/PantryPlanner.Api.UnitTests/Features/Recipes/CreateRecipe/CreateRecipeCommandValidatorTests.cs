using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.Features.Units;

namespace PantryPlanner.Api.UnitTests.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeCommandValidatorTests
{
    private readonly CreateRecipeCommandValidator _validator = new(new InMemoryUnitCatalog());

    [Fact]
    public void Validate_ReturnsNoErrors_ForValidCommand()
    {
        var command = new CreateRecipeCommand
        {
            Title = "Sheet Pan Chicken",
            Servings = 4,
            Ingredients =
            [
                new RecipeIngredientWriteModel
                {
                    Name = "Chicken thighs",
                    ReferenceKey = "chicken",
                    Quantity = 2m,
                    UnitCode = "lb",
                    SortOrder = 1
                }
            ],
            Steps =
            [
                new RecipeStepWriteModel
                {
                    Instruction = "Season the chicken.",
                    SortOrder = 1,
                    IngredientReferenceKeys = ["chicken"]
                }
            ]
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ReturnsExpectedErrors_ForUnknownUnitAndMissingReference()
    {
        var command = new CreateRecipeCommand
        {
            Title = "Bad Recipe",
            Servings = 2,
            Ingredients =
            [
                new RecipeIngredientWriteModel
                {
                    Name = "Salt",
                    ReferenceKey = "salt",
                    Quantity = 1m,
                    UnitCode = "unknown-unit",
                    SortOrder = 1
                }
            ],
            Steps =
            [
                new RecipeStepWriteModel
                {
                    Instruction = "Use the ingredient.",
                    SortOrder = 1,
                    IngredientReferenceKeys = ["pepper"]
                }
            ]
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Ingredient unit code is not supported.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Every step ingredient reference must match an ingredient reference key.");
    }
}
