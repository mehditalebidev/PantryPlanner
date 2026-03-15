using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Media;

public sealed record DeleteRecipeMediaCommand(Guid UserId, Guid RecipeId, Guid MediaId) : IRequest<Result>;
