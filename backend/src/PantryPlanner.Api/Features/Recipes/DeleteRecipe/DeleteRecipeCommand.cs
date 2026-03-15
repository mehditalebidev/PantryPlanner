using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed record DeleteRecipeCommand(Guid UserId, Guid RecipeId) : IRequest<Result>;
