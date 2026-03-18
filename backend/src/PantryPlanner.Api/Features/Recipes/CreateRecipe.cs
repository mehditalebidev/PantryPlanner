using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using PantryPlanner.Api.Features.Units;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.Recipes;

public sealed partial class RecipesController
{
    [HttpPost]
    [ProducesResponseType<RecipeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecipeResponse>> Create([FromBody] CreateRecipeCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record CreateRecipeCommand : IRequest<Result<RecipeResponse>>, IRecipeUpsertRequest
{
    public Guid UserId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int Servings { get; init; }

    public int? PrepTimeMinutes { get; init; }

    public int? CookTimeMinutes { get; init; }

    public string? SourceUrl { get; init; }

    public IReadOnlyCollection<RecipeIngredientWriteModel> Ingredients { get; init; } = [];

    public IReadOnlyCollection<RecipeStepWriteModel> Steps { get; init; } = [];

    public IReadOnlyCollection<RecipeMediaAssetWriteModel> Media { get; init; } = [];
}

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator(IUnitCatalog unitCatalog)
    {
        RecipeValidation.ApplyRecipeRules(this, unitCatalog);
    }
}

public sealed class CreateRecipeHandler : IRequestHandler<CreateRecipeCommand, Result<RecipeResponse>>
{
    private readonly IRecipeContentFactory _recipeContentFactory;
    private readonly PantryPlannerDbContext _dbContext;

    public CreateRecipeHandler(PantryPlannerDbContext dbContext, IRecipeContentFactory recipeContentFactory)
    {
        _dbContext = dbContext;
        _recipeContentFactory = recipeContentFactory;
    }

    public async Task<Result<RecipeResponse>> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = Recipe.Create(
            request.UserId,
            request.Title,
            request.Description,
            request.Servings,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.SourceUrl);

        var contentResult = await _recipeContentFactory.BuildAsync(
            request.UserId,
            null,
            request.Ingredients,
            request.Steps,
            request.Media,
            cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<RecipeResponse>.Failure(contentResult.Error!);
        }

        recipe.ReplaceIngredients(contentResult.Value.Ingredients);
        recipe.ReplaceSteps(contentResult.Value.Steps);
        recipe.ReplaceMediaAssets(contentResult.Value.MediaAssets);

        await _dbContext.AddAsync(recipe, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<RecipeResponse>.Success(request.ToResponse(recipe));
    }
}

file static class CreateRecipeCommandMappings
{
    public static RecipeResponse ToResponse(this CreateRecipeCommand _, Recipe recipe)
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
