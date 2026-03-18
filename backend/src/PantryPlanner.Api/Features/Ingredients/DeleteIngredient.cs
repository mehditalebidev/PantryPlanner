using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed partial class IngredientsController
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteIngredientCommand(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record DeleteIngredientCommand(Guid UserId, Guid IngredientId) : IRequest<Result>;

public sealed class DeleteIngredientHandler : IRequestHandler<DeleteIngredientCommand, Result>
{
    private readonly PantryPlannerDbContext _dbContext;

    public DeleteIngredientHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await _dbContext.Set<Ingredient>()
            .SingleOrDefaultAsync(
                candidate => candidate.UserId == request.UserId && candidate.Id == request.IngredientId,
                cancellationToken);

        if (ingredient is null)
        {
            return Result.Failure(IngredientErrors.NotFound(request.IngredientId));
        }

        var isReferencedByRecipe = await _dbContext.Set<RecipeIngredient>()
            .AnyAsync(recipeIngredient => recipeIngredient.IngredientId == request.IngredientId, cancellationToken);

        if (isReferencedByRecipe)
        {
            return Result.Failure(IngredientErrors.InUseByRecipe());
        }

        _dbContext.Remove(ingredient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
