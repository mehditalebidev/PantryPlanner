using System.Text.Json;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed class RecipeImport
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private RecipeImport()
    {
    }

    private RecipeImport(
        Guid userId,
        string sourceType,
        string sourceUrl,
        string status,
        string draftJson,
        string warningsJson)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        SourceType = sourceType;
        SourceUrl = sourceUrl;
        Status = status;
        DraftJson = draftJson;
        WarningsJson = warningsJson;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string SourceType { get; private set; } = string.Empty;

    public string SourceUrl { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public string DraftJson { get; private set; } = string.Empty;

    public string WarningsJson { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static RecipeImport CreateFromUrl(
        Guid userId,
        string sourceUrl,
        RecipeImportDraft draft,
        IReadOnlyCollection<string> warnings)
    {
        return new RecipeImport(
            userId,
            RecipeImportSourceTypes.Url,
            sourceUrl,
            RecipeImportStatuses.NeedsReview,
            Serialize(draft),
            Serialize(warnings));
    }

    public RecipeImportDraft GetDraft()
    {
        return JsonSerializer.Deserialize<RecipeImportDraft>(DraftJson, SerializerOptions)
            ?? new RecipeImportDraft();
    }

    public IReadOnlyCollection<string> GetWarnings()
    {
        return JsonSerializer.Deserialize<IReadOnlyCollection<string>>(WarningsJson, SerializerOptions)
            ?? [];
    }

    private static string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, SerializerOptions);
    }
}
