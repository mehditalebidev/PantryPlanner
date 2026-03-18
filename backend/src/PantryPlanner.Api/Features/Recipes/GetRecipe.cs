using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.Recipes;

public sealed partial class RecipesController
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecipeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRecipeQuery(User.GetRequiredUserId(), id);
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record GetRecipeQuery(Guid UserId, Guid RecipeId) : IRequest<Result<RecipeResponse>>;

public sealed class GetRecipeHandler : IRequestHandler<GetRecipeQuery, Result<RecipeResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public GetRecipeHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<RecipeResponse>> Handle(GetRecipeQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Set<Recipe>()
            .AsNoTracking()
            .IncludeRecipeDetails()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result<RecipeResponse>.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        return Result<RecipeResponse>.Success(request.ToResponse(recipe));
    }
}

file static class GetRecipeQueryMappings
{
    public static RecipeResponse ToResponse(this GetRecipeQuery _, Recipe recipe)
    {
        return new RecipeResponse(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.Servings,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.SourceUrl,
            recipe.Ingredients
                .OrderBy(ingredient => ingredient.SortOrder)
                .Select(ingredient => ToResponse(ingredient))
                .ToArray(),
            recipe.Steps
                .OrderBy(step => step.SortOrder)
                .Select(step => ToResponse(step))
                .ToArray(),
            recipe.MediaAssets
                .OrderBy(mediaAsset => mediaAsset.SortOrder)
                .Select(mediaAsset => ToResponse(mediaAsset))
                .ToArray(),
            recipe.CreatedAt,
            recipe.UpdatedAt);
    }

    private static RecipeIngredientResponse ToResponse(RecipeIngredient ingredient)
    {
        return new RecipeIngredientResponse(
            ingredient.Id,
            ingredient.IngredientId,
            ingredient.Ingredient.Name,
            ingredient.ReferenceKey,
            ingredient.Quantity,
            ingredient.UnitCode,
            ingredient.NormalizedQuantity,
            ingredient.NormalizedUnitCode,
            ingredient.PreparationNote,
            ingredient.SortOrder);
    }

    private static RecipeStepResponse ToResponse(RecipeStep step)
    {
        return new RecipeStepResponse(
            step.Id,
            step.Instruction,
            step.SortOrder,
            step.DurationMinutes,
            step.IngredientReferences
                .Select(reference => new RecipeStepIngredientReferenceResponse(
                    reference.RecipeIngredientId,
                    reference.RecipeIngredient.IngredientId,
                    reference.RecipeIngredient.ReferenceKey))
                .ToArray());
    }

    private static RecipeMediaAssetResponse ToResponse(RecipeMediaAsset mediaAsset)
    {
        return new RecipeMediaAssetResponse(
            mediaAsset.Id,
            mediaAsset.Kind,
            mediaAsset.StorageKey,
            mediaAsset.Url,
            mediaAsset.ContentType,
            mediaAsset.Caption,
            mediaAsset.SortOrder);
    }
}
