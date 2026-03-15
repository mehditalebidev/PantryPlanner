using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed record ListRecipesQuery(Guid UserId) : IRequest<Result<IReadOnlyCollection<RecipeResponse>>>;
