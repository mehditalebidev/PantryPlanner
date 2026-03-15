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

        var recipeMediaAssets = mediaRequests
            .OrderBy(mediaAsset => mediaAsset.SortOrder)
            .Select(mediaAsset => RecipeMediaAsset.Create(
                mediaAsset.Kind,
                mediaAsset.StorageKey,
                mediaAsset.Url,
                mediaAsset.Caption,
                mediaAsset.SortOrder))
            .ToArray();

        return Result<RecipeContentDraft>.Success(new RecipeContentDraft(recipeIngredients, recipeSteps, recipeMediaAssets));
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
