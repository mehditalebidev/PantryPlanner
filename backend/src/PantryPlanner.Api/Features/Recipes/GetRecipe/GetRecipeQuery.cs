using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed record GetRecipeQuery(Guid UserId, Guid RecipeId) : IRequest<Result<RecipeResponse>>;
