using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class DeleteIngredientHandler : IRequestHandler<DeleteIngredientCommand, Result>
{
    private readonly IRepository _repository;

    public DeleteIngredientHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await _repository.Query<Ingredient>()
            .SingleOrDefaultAsync(
                candidate => candidate.UserId == request.UserId && candidate.Id == request.IngredientId,
                cancellationToken);

        if (ingredient is null)
        {
            return Result.Failure(IngredientErrors.NotFound(request.IngredientId));
        }

        var isReferencedByRecipe = await _repository.Query<RecipeIngredient>()
            .AnyAsync(recipeIngredient => recipeIngredient.IngredientId == request.IngredientId, cancellationToken);

        if (isReferencedByRecipe)
        {
            return Result.Failure(IngredientErrors.InUseByRecipe());
        }

        _repository.Remove(ingredient);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
