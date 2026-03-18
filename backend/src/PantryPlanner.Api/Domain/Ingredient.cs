namespace PantryPlanner.Api.Features.Ingredients;

public sealed class Ingredient
{
    private Ingredient()
    {
    }

    private Ingredient(Guid userId, string name, string normalizedName)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        NormalizedName = normalizedName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Ingredient Create(Guid userId, string name)
    {
        var normalizedName = NormalizeName(name);
        return new Ingredient(userId, NormalizeDisplayName(name), normalizedName);
    }

    public void Rename(string name)
    {
        Name = NormalizeDisplayName(name);
        NormalizedName = NormalizeName(name);
        UpdatedAt = DateTime.UtcNow;
    }

    public static string NormalizeName(string name)
    {
        return NormalizeDisplayName(name).ToLowerInvariant();
    }

    private static string NormalizeDisplayName(string name)
    {
        return string.Join(' ', name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
