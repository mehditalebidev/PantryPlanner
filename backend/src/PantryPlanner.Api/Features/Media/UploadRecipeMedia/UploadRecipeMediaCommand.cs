using MediatR;
using Microsoft.AspNetCore.Http;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Media;

public sealed record UploadRecipeMediaCommand : IRequest<Result<RecipeMediaAssetResponse>>
{
    public Guid UserId { get; init; }

    public Guid RecipeId { get; init; }

    public string Kind { get; init; } = string.Empty;

    public string? Caption { get; init; }

    public int SortOrder { get; init; }

    public IFormFile File { get; init; } = null!;
}
