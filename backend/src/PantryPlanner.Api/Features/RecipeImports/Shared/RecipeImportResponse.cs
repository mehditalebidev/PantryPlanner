namespace PantryPlanner.Api.Features.RecipeImports;

public sealed record RecipeImportResponse(
    Guid Id,
    string SourceType,
    string SourceUrl,
    string Status,
    RecipeImportDraft Draft,
    IReadOnlyCollection<string> Warnings,
    DateTime CreatedAt,
    DateTime UpdatedAt);
