using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.Units;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeContentFactory : IRecipeContentFactory
{
    private readonly IMeasurementNormalizer _measurementNormalizer;
    private readonly IRepository _repository;

    public RecipeContentFactory(IRepository repository, IMeasurementNormalizer measurementNormalizer)
    {
        _repository = repository;
        _measurementNormalizer = measurementNormalizer;
    }

    public async Task<Result<RecipeContentDraft>> BuildAsync(
        Guid userId,
        Guid? recipeId,
        IReadOnlyCollection<RecipeIngredientWriteModel> ingredientRequests,
        IReadOnlyCollection<RecipeStepWriteModel> stepRequests,
        IReadOnlyCollection<RecipeMediaAssetWriteModel> mediaRequests,
        CancellationToken cancellationToken)
    {
        var existingIngredientsById = await LoadIngredientsByIdAsync(userId, ingredientRequests, cancellationToken);
        if (existingIngredientsById is null)
        {
            return Result<RecipeContentDraft>.Failure(RecipeErrors.InvalidIngredientReference());
        }

        var existingMediaByStorageKey = await LoadMediaByStorageKeyAsync(userId, recipeId, mediaRequests, cancellationToken);
        if (existingMediaByStorageKey is null)
        {
            return Result<RecipeContentDraft>.Failure(RecipeErrors.InvalidMediaReference());
        }

        var existingIngredientsByName = await LoadIngredientsByNameAsync(userId, ingredientRequests, cancellationToken);

        var recipeIngredients = new List<RecipeIngredient>(ingredientRequests.Count);
        var recipeIngredientsByReferenceKey = new Dictionary<string, RecipeIngredient>(StringComparer.OrdinalIgnoreCase);

        foreach (var ingredientRequest in ingredientRequests.OrderBy(ingredient => ingredient.SortOrder))
        {
            Ingredient ingredient;

            if (ingredientRequest.IngredientId is Guid ingredientId)
            {
                ingredient = existingIngredientsById[ingredientId];
            }
            else
            {
                var normalizedName = Ingredient.NormalizeName(ingredientRequest.Name!);

                if (!existingIngredientsByName.TryGetValue(normalizedName, out ingredient!))
                {
                    ingredient = Ingredient.Create(userId, ingredientRequest.Name!);
                    existingIngredientsByName[normalizedName] = ingredient;
                    await _repository.AddAsync(ingredient, cancellationToken);
                }
            }

            var normalizedMeasurement = _measurementNormalizer.Normalize(ingredientRequest.Quantity, ingredientRequest.UnitCode);

            var recipeIngredient = RecipeIngredient.Create(
                ingredient,
                ingredientRequest.ReferenceKey,
                ingredientRequest.Quantity,
                ingredientRequest.UnitCode,
                normalizedMeasurement?.Quantity,
                normalizedMeasurement?.UnitCode,
                ingredientRequest.PreparationNote,
                ingredientRequest.SortOrder);

            recipeIngredients.Add(recipeIngredient);
            recipeIngredientsByReferenceKey[recipeIngredient.ReferenceKey] = recipeIngredient;
        }

        var recipeSteps = stepRequests
            .OrderBy(step => step.SortOrder)
            .Select(step => RecipeStep.Create(
                step.Instruction,
                step.SortOrder,
                step.DurationMinutes,
                step.IngredientReferenceKeys.Select(referenceKey => recipeIngredientsByReferenceKey[referenceKey])))
            .ToArray();

        var recipeMediaAssets = new List<RecipeMediaAsset>(mediaRequests.Count);

        foreach (var mediaAsset in mediaRequests.OrderBy(mediaAsset => mediaAsset.SortOrder))
        {
            if (!string.IsNullOrWhiteSpace(mediaAsset.StorageKey))
            {
                var storageKey = mediaAsset.StorageKey.Trim();
                var existingMedia = existingMediaByStorageKey[storageKey];

                if (!string.Equals(existingMedia.Kind, mediaAsset.Kind, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<RecipeContentDraft>.Failure(RecipeErrors.InvalidMediaReference());
                }

                recipeMediaAssets.Add(RecipeMediaAsset.Create(
                    existingMedia.Kind,
                    existingMedia.StorageKey,
                    existingMedia.Url,
                    existingMedia.ContentType,
                    mediaAsset.Caption,
                    mediaAsset.SortOrder));

                continue;
            }

            recipeMediaAssets.Add(RecipeMediaAsset.Create(
                mediaAsset.Kind,
                null,
                mediaAsset.Url,
                mediaAsset.ContentType,
                mediaAsset.Caption,
                mediaAsset.SortOrder));
        }

        return Result<RecipeContentDraft>.Success(new RecipeContentDraft(recipeIngredients, recipeSteps, recipeMediaAssets));
    }

    private async Task<Dictionary<string, RecipeMediaAsset>?> LoadMediaByStorageKeyAsync(
        Guid userId,
        Guid? recipeId,
        IReadOnlyCollection<RecipeMediaAssetWriteModel> mediaRequests,
        CancellationToken cancellationToken)
    {
        var storageKeys = mediaRequests
            .Where(mediaAsset => !string.IsNullOrWhiteSpace(mediaAsset.StorageKey))
            .Select(mediaAsset => mediaAsset.StorageKey!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (storageKeys.Length == 0)
        {
            return new Dictionary<string, RecipeMediaAsset>(StringComparer.Ordinal);
        }

        if (!recipeId.HasValue)
        {
            return null;
        }

        var mediaByStorageKey = await _repository.Query<RecipeMediaAsset>()
            .Where(mediaAsset =>
                mediaAsset.RecipeId == recipeId.Value &&
                mediaAsset.Recipe.UserId == userId &&
                mediaAsset.StorageKey != null &&
                storageKeys.Contains(mediaAsset.StorageKey))
            .ToDictionaryAsync(mediaAsset => mediaAsset.StorageKey!, cancellationToken);

        return mediaByStorageKey.Count == storageKeys.Length ? mediaByStorageKey : null;
    }

    private async Task<Dictionary<Guid, Ingredient>?> LoadIngredientsByIdAsync(
        Guid userId,
        IReadOnlyCollection<RecipeIngredientWriteModel> ingredientRequests,
        CancellationToken cancellationToken)
    {
        var ingredientIds = ingredientRequests
            .Where(ingredient => ingredient.IngredientId.HasValue)
            .Select(ingredient => ingredient.IngredientId!.Value)
            .Distinct()
            .ToArray();

        var ingredientsById = ingredientIds.Length == 0
            ? []
            : await _repository.Query<Ingredient>()
                .Where(ingredient => ingredient.UserId == userId && ingredientIds.Contains(ingredient.Id))
                .ToDictionaryAsync(ingredient => ingredient.Id, cancellationToken);

        return ingredientsById.Count == ingredientIds.Length ? ingredientsById : null;
    }

    private async Task<Dictionary<string, Ingredient>> LoadIngredientsByNameAsync(
        Guid userId,
        IReadOnlyCollection<RecipeIngredientWriteModel> ingredientRequests,
        CancellationToken cancellationToken)
    {
        var normalizedNames = ingredientRequests
            .Where(ingredient => !ingredient.IngredientId.HasValue)
            .Select(ingredient => Ingredient.NormalizeName(ingredient.Name!))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedNames.Length == 0)
        {
            return new Dictionary<string, Ingredient>(StringComparer.Ordinal);
        }

        return await _repository.Query<Ingredient>()
            .Where(ingredient => ingredient.UserId == userId && normalizedNames.Contains(ingredient.NormalizedName))
            .ToDictionaryAsync(ingredient => ingredient.NormalizedName, cancellationToken);
    }
}
