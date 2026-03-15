using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed record CreateRecipeCommand : IRequest<Result<RecipeResponse>>, IRecipeUpsertRequest
{
    public Guid UserId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int Servings { get; init; }

    public int? PrepTimeMinutes { get; init; }

    public int? CookTimeMinutes { get; init; }

    public string? SourceUrl { get; init; }

    public IReadOnlyCollection<RecipeIngredientWriteModel> Ingredients { get; init; } = [];

    public IReadOnlyCollection<RecipeStepWriteModel> Steps { get; init; } = [];

    public IReadOnlyCollection<RecipeMediaAssetWriteModel> Media { get; init; } = [];
}
