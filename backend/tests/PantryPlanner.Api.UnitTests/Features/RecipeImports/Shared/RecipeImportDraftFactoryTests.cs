using PantryPlanner.Api.Features.RecipeImports;

namespace PantryPlanner.Api.UnitTests.Features.RecipeImports.Shared;

public sealed class RecipeImportDraftFactoryTests
{
    [Fact]
    public void CreateFromUrl_InferesStarterDraftFromSlug()
    {
        var factory = new RecipeImportDraftFactory();

        var result = factory.CreateFromUrl("https://example.com/recipes/sheet-pan-chicken");

        Assert.Equal("Sheet Pan Chicken", result.Draft.Title);
        Assert.Equal("https://example.com/recipes/sheet-pan-chicken", result.Draft.SourceUrl);
        Assert.Empty(result.Draft.Ingredients);
        Assert.Empty(result.Draft.Steps);
        Assert.Empty(result.Draft.Media);
        Assert.Contains(RecipeImportWarnings.ReviewRequired, result.Warnings);
    }
}
