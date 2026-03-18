using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using PantryPlanner.Api.Features.Units;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Media;

namespace PantryPlanner.Api.Features.Recipes;

public sealed partial class RecipesController
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType<RecipeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeResponse>> Update(
        Guid id,
        [FromBody] UpdateRecipeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { RecipeId = id, UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record UpdateRecipeCommand : IRequest<Result<RecipeResponse>>, IRecipeUpsertRequest
{
    public Guid RecipeId { get; init; }

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

public sealed class UpdateRecipeCommandValidator : AbstractValidator<UpdateRecipeCommand>
{
    public UpdateRecipeCommandValidator(IUnitCatalog unitCatalog)
    {
        RuleFor(command => command.RecipeId)
            .NotEmpty()
            .WithMessage("RecipeId is required.");

        RecipeValidation.ApplyRecipeRules(this, unitCatalog);
    }
}

public sealed class UpdateRecipeHandler : IRequestHandler<UpdateRecipeCommand, Result<RecipeResponse>>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly IRecipeContentFactory _recipeContentFactory;
    private readonly PantryPlannerDbContext _dbContext;

    public UpdateRecipeHandler(PantryPlannerDbContext dbContext, IRecipeContentFactory recipeContentFactory, IMediaStorage mediaStorage)
    {
        _dbContext = dbContext;
        _recipeContentFactory = recipeContentFactory;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result<RecipeResponse>> Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Set<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result<RecipeResponse>.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        var contentResult = await _recipeContentFactory.BuildAsync(
            request.UserId,
            request.RecipeId,
            request.Ingredients,
            request.Steps,
            request.Media,
            cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<RecipeResponse>.Failure(contentResult.Error!);
        }

        recipe.UpdateDetails(
            request.Title,
            request.Description,
            request.Servings,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.SourceUrl);

        var existingMediaStorageKeys = await _dbContext.RecipeMediaAssets
            .Where(mediaAsset => mediaAsset.RecipeId == request.RecipeId && mediaAsset.StorageKey != null)
            .Select(mediaAsset => mediaAsset.StorageKey!)
            .ToArrayAsync(cancellationToken);

        var retainedMediaStorageKeys = request.Media
            .Where(mediaAsset => !string.IsNullOrWhiteSpace(mediaAsset.StorageKey))
            .Select(mediaAsset => mediaAsset.StorageKey!.Trim())
            .ToHashSet(StringComparer.Ordinal);

        var removedMediaStorageKeys = existingMediaStorageKeys
            .Where(storageKey => !retainedMediaStorageKeys.Contains(storageKey))
            .ToArray();

        var existingStepIds = await _dbContext.RecipeSteps
            .Where(step => step.RecipeId == request.RecipeId)
            .Select(step => step.Id)
            .ToArrayAsync(cancellationToken);

        if (existingStepIds.Length > 0)
        {
            await _dbContext.RecipeStepIngredientReferences
                .Where(reference => existingStepIds.Contains(reference.RecipeStepId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await _dbContext.RecipeMediaAssets
            .Where(mediaAsset => mediaAsset.RecipeId == request.RecipeId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.RecipeSteps
            .Where(step => step.RecipeId == request.RecipeId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.RecipeIngredients
            .Where(ingredient => ingredient.RecipeId == request.RecipeId)
            .ExecuteDeleteAsync(cancellationToken);

        recipe.ReplaceIngredients(contentResult.Value.Ingredients);
        recipe.ReplaceSteps(contentResult.Value.Steps);
        recipe.ReplaceMediaAssets(contentResult.Value.MediaAssets);

        foreach (var ingredient in contentResult.Value.Ingredients)
        {
            _dbContext.Entry(ingredient).State = EntityState.Added;
        }

        foreach (var step in contentResult.Value.Steps)
        {
            _dbContext.Entry(step).State = EntityState.Added;

            foreach (var ingredientReference in step.IngredientReferences)
            {
                _dbContext.Entry(ingredientReference).State = EntityState.Added;
            }
        }

        foreach (var mediaAsset in contentResult.Value.MediaAssets)
        {
            _dbContext.Entry(mediaAsset).State = EntityState.Added;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var storageKey in removedMediaStorageKeys)
        {
            await _mediaStorage.DeleteIfExistsAsync(storageKey, cancellationToken);
        }

        return Result<RecipeResponse>.Success(request.ToResponse(recipe));
    }
}

file static class UpdateRecipeCommandMappings
{
    public static RecipeResponse ToResponse(this UpdateRecipeCommand _, Recipe recipe)
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
