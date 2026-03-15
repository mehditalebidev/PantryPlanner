using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class ListRecipesHandler : IRequestHandler<ListRecipesQuery, Result<IReadOnlyCollection<RecipeResponse>>>
{
    private readonly IRepository _repository;

    public ListRecipesHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyCollection<RecipeResponse>>> Handle(ListRecipesQuery request, CancellationToken cancellationToken)
    {
        var recipes = await _repository.Query<Recipe>()
            .AsNoTracking()
            .Where(recipe => recipe.UserId == request.UserId)
            .OrderByDescending(recipe => recipe.UpdatedAt)
            .IncludeRecipeDetails()
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<RecipeResponse>>.Success(recipes.Select(recipe => recipe.ToResponse()).ToArray());
    }
}
