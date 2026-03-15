using PantryPlanner.Api.Features.RecipeImports;

namespace PantryPlanner.Api.UnitTests.Features.RecipeImports.CreateRecipeImport;

public sealed class CreateRecipeImportCommandValidatorTests
{
    private readonly CreateRecipeImportCommandValidator _validator = new();

    [Fact]
    public void Validate_ReturnsNoErrors_ForValidCommand()
    {
        var command = new CreateRecipeImportCommand
        {
            SourceUrl = "https://example.com/recipes/sheet-pan-chicken"
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ReturnsExpectedErrors_ForInvalidSourceUrl()
    {
        var command = new CreateRecipeImportCommand
        {
            SourceUrl = "not-a-url"
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "SourceUrl must be a valid absolute URL.");
    }
}
