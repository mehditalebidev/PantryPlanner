using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed record GetIngredientQuery(Guid UserId, Guid IngredientId) : IRequest<Result<IngredientResponse>>;
