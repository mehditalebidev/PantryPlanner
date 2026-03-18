namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeMediaAsset
{
    private RecipeMediaAsset()
    {
    }

    private RecipeMediaAsset(string kind, string? storageKey, string url, string? contentType, string? caption, int sortOrder)
    {
        Id = Guid.NewGuid();
        Kind = kind.Trim();
        StorageKey = string.IsNullOrWhiteSpace(storageKey) ? null : storageKey.Trim();
        Url = url.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim();
        Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }

    public Guid RecipeId { get; private set; }

    public Recipe Recipe { get; private set; } = null!;

    public string Kind { get; private set; } = string.Empty;

    public string? StorageKey { get; private set; }

    public string Url { get; private set; } = string.Empty;

    public string? ContentType { get; private set; }

    public string? Caption { get; private set; }

    public int SortOrder { get; private set; }

    public static RecipeMediaAsset Create(string kind, string? storageKey, string url, string? contentType, string? caption, int sortOrder)
    {
        return new RecipeMediaAsset(kind, storageKey, url, contentType, caption, sortOrder);
    }
}
