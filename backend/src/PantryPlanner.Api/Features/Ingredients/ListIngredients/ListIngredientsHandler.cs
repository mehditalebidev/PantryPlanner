using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class ListIngredientsHandler : IRequestHandler<ListIngredientsQuery, Result<IReadOnlyCollection<IngredientResponse>>>
{
    private readonly IRepository _repository;

    public ListIngredientsHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyCollection<IngredientResponse>>> Handle(ListIngredientsQuery request, CancellationToken cancellationToken)
    {
        var ingredients = await _repository.Query<Ingredient>()
            .AsNoTracking()
            .Where(ingredient => ingredient.UserId == request.UserId)
            .OrderBy(ingredient => ingredient.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<IngredientResponse>>.Success(ingredients.Select(ingredient => ingredient.ToResponse()).ToArray());
    }
}
