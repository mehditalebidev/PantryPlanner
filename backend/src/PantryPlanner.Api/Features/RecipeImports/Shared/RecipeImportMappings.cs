namespace PantryPlanner.Api.Features.RecipeImports;

public static class RecipeImportMappings
{
    public static RecipeImportResponse ToResponse(this RecipeImport recipeImport)
    {
        return new RecipeImportResponse(
            recipeImport.Id,
            recipeImport.SourceType,
            recipeImport.SourceUrl,
            recipeImport.Status,
            recipeImport.GetDraft(),
            recipeImport.GetWarnings(),
            recipeImport.CreatedAt,
            recipeImport.UpdatedAt);
    }
}
