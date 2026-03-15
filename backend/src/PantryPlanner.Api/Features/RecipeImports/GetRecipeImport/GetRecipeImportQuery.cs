using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed record GetRecipeImportQuery(Guid UserId, Guid RecipeImportId) : IRequest<Result<RecipeImportResponse>>;
