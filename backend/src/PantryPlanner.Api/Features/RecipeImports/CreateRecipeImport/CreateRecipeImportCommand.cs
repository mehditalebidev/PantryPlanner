using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed record CreateRecipeImportCommand : IRequest<Result<RecipeImportResponse>>
{
    public Guid UserId { get; init; }

    public string SourceUrl { get; init; } = string.Empty;
}
