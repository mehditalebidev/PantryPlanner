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
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<RecipeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<RecipeResponse>>> List(CancellationToken cancellationToken)
    {
        var query = new ListRecipesQuery(User.GetRequiredUserId());
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record ListRecipesQuery(Guid UserId) : IRequest<Result<IReadOnlyCollection<RecipeResponse>>>;

public sealed class ListRecipesHandler : IRequestHandler<ListRecipesQuery, Result<IReadOnlyCollection<RecipeResponse>>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public ListRecipesHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<RecipeResponse>>> Handle(ListRecipesQuery request, CancellationToken cancellationToken)
    {
        var recipes = await _dbContext.Set<Recipe>()
            .AsNoTracking()
            .Where(recipe => recipe.UserId == request.UserId)
            .OrderByDescending(recipe => recipe.UpdatedAt)
            .IncludeRecipeDetails()
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<RecipeResponse>>.Success(request.ToResponse(recipes));
    }
}

file static class ListRecipesQueryMappings
{
    public static IReadOnlyCollection<RecipeResponse> ToResponse(this ListRecipesQuery _, IEnumerable<Recipe> recipes)
    {
        return recipes.Select(recipe => _.ToResponse(recipe)).ToArray();
    }

    public static RecipeResponse ToResponse(this ListRecipesQuery _, Recipe recipe)
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
