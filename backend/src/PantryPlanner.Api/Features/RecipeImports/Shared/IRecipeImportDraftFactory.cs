namespace PantryPlanner.Api.Features.RecipeImports;

public interface IRecipeImportDraftFactory
{
    RecipeImportDraftBuildResult CreateFromUrl(string sourceUrl);
}
