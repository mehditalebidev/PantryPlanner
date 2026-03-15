namespace PantryPlanner.Api.Features.Recipes;

public sealed record RecipeResponse(
    Guid Id,
    string Title,
    string? Description,
    int Servings,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    string? SourceUrl,
    IReadOnlyCollection<RecipeIngredientResponse> Ingredients,
    IReadOnlyCollection<RecipeStepResponse> Steps,
    IReadOnlyCollection<RecipeMediaAssetResponse> Media,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record RecipeIngredientResponse(
    Guid Id,
    Guid IngredientId,
    string Name,
    string ReferenceKey,
    decimal Quantity,
    string UnitCode,
    decimal? NormalizedQuantity,
    string? NormalizedUnitCode,
    string? PreparationNote,
    int SortOrder);

public sealed record RecipeStepResponse(
    Guid Id,
    string Instruction,
    int SortOrder,
    int? DurationMinutes,
    IReadOnlyCollection<RecipeStepIngredientReferenceResponse> IngredientReferences);

public sealed record RecipeStepIngredientReferenceResponse(
    Guid RecipeIngredientId,
    Guid IngredientId,
    string ReferenceKey);

public sealed record RecipeMediaAssetResponse(
    Guid Id,
    string Kind,
    string? StorageKey,
    string Url,
    string? ContentType,
    string? Caption,
    int SortOrder);
