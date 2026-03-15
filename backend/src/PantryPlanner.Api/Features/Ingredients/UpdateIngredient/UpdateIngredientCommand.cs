using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed record UpdateIngredientCommand : IRequest<Result<IngredientResponse>>
{
    public Guid UserId { get; init; }

    public Guid IngredientId { get; init; }

    public string Name { get; init; } = string.Empty;
}
