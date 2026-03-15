namespace PantryPlanner.Api.Features.RecipeImports;

public static class RecipeImportSourceTypes
{
    public const string Url = "url";
}

public static class RecipeImportStatuses
{
    public const string NeedsReview = "needs_review";
}

public static class RecipeImportWarnings
{
    public const string ReviewRequired = "This import foundation only infers a starter draft from the source URL. Review and complete the recipe before saving it.";
}
