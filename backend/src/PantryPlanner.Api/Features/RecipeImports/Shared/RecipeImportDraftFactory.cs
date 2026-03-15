namespace PantryPlanner.Api.Features.RecipeImports;

public sealed class RecipeImportDraftFactory : IRecipeImportDraftFactory
{
    public RecipeImportDraftBuildResult CreateFromUrl(string sourceUrl)
    {
        var draft = new RecipeImportDraft
        {
            Title = InferTitle(sourceUrl),
            SourceUrl = sourceUrl,
            Ingredients = [],
            Steps = [],
            Media = []
        };

        return new RecipeImportDraftBuildResult(draft, [RecipeImportWarnings.ReviewRequired]);
    }

    private static string InferTitle(string sourceUrl)
    {
        var uri = new Uri(sourceUrl);
        var lastSegment = uri.Segments
            .Select(segment => segment.Trim('/'))
            .LastOrDefault(segment => !string.IsNullOrWhiteSpace(segment));

        if (string.IsNullOrWhiteSpace(lastSegment))
        {
            return "Imported Recipe";
        }

        var slug = Uri.UnescapeDataString(lastSegment);
        var extensionIndex = slug.LastIndexOf('.');

        if (extensionIndex > 0)
        {
            slug = slug[..extensionIndex];
        }

        var words = slug
            .Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ToTitleWord)
            .ToArray();

        return words.Length == 0 ? "Imported Recipe" : string.Join(' ', words);
    }

    private static string ToTitleWord(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var lowerValue = value.ToLowerInvariant();
        return char.ToUpperInvariant(lowerValue[0]) + lowerValue[1..];
    }
}
