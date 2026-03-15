using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class GetRecipeHandler : IRequestHandler<GetRecipeQuery, Result<RecipeResponse>>
{
    private readonly IRepository _repository;

    public GetRecipeHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<RecipeResponse>> Handle(GetRecipeQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _repository.Query<Recipe>()
            .AsNoTracking()
            .IncludeRecipeDetails()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result<RecipeResponse>.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        return Result<RecipeResponse>.Success(recipe.ToResponse());
    }
}
