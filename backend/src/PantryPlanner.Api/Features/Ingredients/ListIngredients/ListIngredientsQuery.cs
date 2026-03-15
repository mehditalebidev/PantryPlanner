using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed record ListIngredientsQuery(Guid UserId) : IRequest<Result<IReadOnlyCollection<IngredientResponse>>>;
