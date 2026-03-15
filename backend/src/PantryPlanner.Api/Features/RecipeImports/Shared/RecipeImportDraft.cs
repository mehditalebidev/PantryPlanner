using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed record RecipeImportDraft
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public int? Servings { get; init; }

    public int? PrepTimeMinutes { get; init; }

    public int? CookTimeMinutes { get; init; }

    public string? SourceUrl { get; init; }

    public IReadOnlyCollection<RecipeIngredientWriteModel> Ingredients { get; init; } = [];

    public IReadOnlyCollection<RecipeStepWriteModel> Steps { get; init; } = [];

    public IReadOnlyCollection<RecipeMediaAssetWriteModel> Media { get; init; } = [];
}

public sealed record RecipeImportDraftBuildResult(RecipeImportDraft Draft, IReadOnlyCollection<string> Warnings);
