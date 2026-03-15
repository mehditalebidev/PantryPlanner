using FluentValidation;
using PantryPlanner.Api.Features.Units;

namespace PantryPlanner.Api.Features.Recipes;

internal static class RecipeValidation
{
    public static void ApplyRecipeRules<T>(AbstractValidator<T> validator, IUnitCatalog unitCatalog)
        where T : IRecipeUpsertRequest
    {
        validator.RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title must be 200 characters or fewer.");

        validator.RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithMessage("Description must be 2000 characters or fewer.");

        validator.RuleFor(command => command.Servings)
            .GreaterThan(0)
            .WithMessage("Servings must be greater than 0.");

        validator.RuleFor(command => command.PrepTimeMinutes)
            .GreaterThanOrEqualTo(0)
            .When(command => command.PrepTimeMinutes.HasValue)
            .WithMessage("PrepTimeMinutes must be 0 or greater.");

        validator.RuleFor(command => command.CookTimeMinutes)
            .GreaterThanOrEqualTo(0)
            .When(command => command.CookTimeMinutes.HasValue)
            .WithMessage("CookTimeMinutes must be 0 or greater.");

        validator.RuleFor(command => command.SourceUrl)
            .Must(BeAValidAbsoluteUrl)
            .When(command => !string.IsNullOrWhiteSpace(command.SourceUrl))
            .WithMessage("SourceUrl must be a valid absolute URL.");

        validator.RuleFor(command => command.Ingredients)
            .NotEmpty()
            .WithMessage("At least one ingredient is required.");

        validator.RuleForEach(command => command.Ingredients)
            .SetValidator(new RecipeIngredientWriteModelValidator(unitCatalog));

        validator.RuleFor(command => command.Ingredients)
            .Must(HaveDistinctReferenceKeys)
            .WithMessage("Ingredient reference keys must be unique.");

        validator.RuleFor(command => command.Ingredients)
            .Must(HaveDistinctIngredientSortOrders)
            .WithMessage("Ingredient sort orders must be unique.");

        validator.RuleFor(command => command.Steps)
            .NotEmpty()
            .WithMessage("At least one step is required.");

        validator.RuleForEach(command => command.Steps)
            .SetValidator(new RecipeStepWriteModelValidator());

        validator.RuleFor(command => command.Steps)
            .Must(HaveDistinctStepSortOrders)
            .WithMessage("Step sort orders must be unique.");

        validator.RuleFor(command => command)
            .Must(HaveValidStepIngredientReferences)
            .WithMessage("Every step ingredient reference must match an ingredient reference key.");

        validator.RuleForEach(command => command.Media)
            .SetValidator(new RecipeMediaAssetWriteModelValidator());

        validator.RuleFor(command => command.Media)
            .Must(HaveDistinctMediaSortOrders)
            .WithMessage("Media sort orders must be unique.");
    }

    private static bool BeAValidAbsoluteUrl(string? sourceUrl)
    {
        return Uri.TryCreate(sourceUrl, UriKind.Absolute, out _);
    }

    private static bool HaveDistinctReferenceKeys(IReadOnlyCollection<RecipeIngredientWriteModel> ingredients)
    {
        return ingredients.Count == ingredients
            .Select(ingredient => ingredient.ReferenceKey.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private static bool HaveDistinctIngredientSortOrders(IReadOnlyCollection<RecipeIngredientWriteModel> ingredients)
    {
        return ingredients.Count == ingredients.Select(ingredient => ingredient.SortOrder).Distinct().Count();
    }

    private static bool HaveDistinctStepSortOrders(IReadOnlyCollection<RecipeStepWriteModel> steps)
    {
        return steps.Count == steps.Select(step => step.SortOrder).Distinct().Count();
    }

    private static bool HaveValidStepIngredientReferences<T>(T command)
        where T : IRecipeUpsertRequest
    {
        var ingredientReferenceKeys = command.Ingredients
            .Select(ingredient => ingredient.ReferenceKey.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return command.Steps.All(step => step.IngredientReferenceKeys
            .Select(referenceKey => referenceKey.Trim())
            .All(ingredientReferenceKeys.Contains));
    }

    private static bool HaveDistinctMediaSortOrders(IReadOnlyCollection<RecipeMediaAssetWriteModel> media)
    {
        return media.Count == media.Select(mediaAsset => mediaAsset.SortOrder).Distinct().Count();
    }
}

internal sealed class RecipeIngredientWriteModelValidator : AbstractValidator<RecipeIngredientWriteModel>
{
    public RecipeIngredientWriteModelValidator(IUnitCatalog unitCatalog)
    {
        RuleFor(ingredient => ingredient.Name)
            .NotEmpty()
            .WithMessage("Ingredient name is required when IngredientId is not provided.")
            .MaximumLength(200)
            .WithMessage("Ingredient name must be 200 characters or fewer.")
            .When(ingredient => !ingredient.IngredientId.HasValue);

        RuleFor(ingredient => ingredient.ReferenceKey)
            .NotEmpty()
            .WithMessage("Ingredient reference key is required.")
            .MaximumLength(100)
            .WithMessage("Ingredient reference key must be 100 characters or fewer.");

        RuleFor(ingredient => ingredient.Quantity)
            .GreaterThan(0)
            .WithMessage("Ingredient quantity must be greater than 0.");

        RuleFor(ingredient => ingredient.UnitCode)
            .NotEmpty()
            .WithMessage("Ingredient unit code is required.")
            .Must(unitCode => unitCatalog.TryGet(unitCode, out _))
            .WithMessage("Ingredient unit code is not supported.");

        RuleFor(ingredient => ingredient.PreparationNote)
            .MaximumLength(500)
            .WithMessage("PreparationNote must be 500 characters or fewer.");

        RuleFor(ingredient => ingredient.SortOrder)
            .GreaterThan(0)
            .WithMessage("Ingredient sort order must be greater than 0.");
    }
}

internal sealed class RecipeStepWriteModelValidator : AbstractValidator<RecipeStepWriteModel>
{
    public RecipeStepWriteModelValidator()
    {
        RuleFor(step => step.Instruction)
            .NotEmpty()
            .WithMessage("Step instruction is required.")
            .MaximumLength(4000)
            .WithMessage("Step instruction must be 4000 characters or fewer.");

        RuleFor(step => step.SortOrder)
            .GreaterThan(0)
            .WithMessage("Step sort order must be greater than 0.");

        RuleFor(step => step.DurationMinutes)
            .GreaterThan(0)
            .When(step => step.DurationMinutes.HasValue)
            .WithMessage("DurationMinutes must be greater than 0 when provided.");

        RuleFor(step => step.IngredientReferenceKeys)
            .Must(referenceKeys => referenceKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count() == referenceKeys.Count)
            .WithMessage("Step ingredient references must be unique.");
    }
}

internal sealed class RecipeMediaAssetWriteModelValidator : AbstractValidator<RecipeMediaAssetWriteModel>
{
    private static readonly string[] AllowedKinds = ["image", "video"];

    public RecipeMediaAssetWriteModelValidator()
    {
        RuleFor(media => media.Kind)
            .NotEmpty()
            .WithMessage("Media kind is required.")
            .Must(kind => AllowedKinds.Contains(kind, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Media kind must be image or video.");

        RuleFor(media => media.StorageKey)
            .MaximumLength(500)
            .WithMessage("StorageKey must be 500 characters or fewer.");

        RuleFor(media => media.Url)
            .NotEmpty()
            .WithMessage("Media URL is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Media URL must be a valid absolute URL.")
            .MaximumLength(1000)
            .WithMessage("Media URL must be 1000 characters or fewer.");

        RuleFor(media => media.Caption)
            .MaximumLength(500)
            .WithMessage("Caption must be 500 characters or fewer.");

        RuleFor(media => media.SortOrder)
            .GreaterThan(0)
            .WithMessage("Media sort order must be greater than 0.");
    }
}
