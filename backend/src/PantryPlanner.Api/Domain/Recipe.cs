namespace PantryPlanner.Api.Features.Recipes;

public sealed class Recipe
{
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<RecipeStep> _steps = [];
    private readonly List<RecipeMediaAsset> _mediaAssets = [];

    private Recipe()
    {
    }

    private Recipe(
        Guid userId,
        string title,
        string? description,
        int servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? sourceUrl)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Servings = servings;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Servings { get; private set; }

    public int? PrepTimeMinutes { get; private set; }

    public int? CookTimeMinutes { get; private set; }

    public string? SourceUrl { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients;

    public IReadOnlyCollection<RecipeStep> Steps => _steps;

    public IReadOnlyCollection<RecipeMediaAsset> MediaAssets => _mediaAssets;

    public static Recipe Create(
        Guid userId,
        string title,
        string? description,
        int servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? sourceUrl)
    {
        return new Recipe(userId, title, description, servings, prepTimeMinutes, cookTimeMinutes, sourceUrl);
    }

    public void UpdateDetails(
        string title,
        string? description,
        int servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? sourceUrl)
    {
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Servings = servings;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceIngredients(IEnumerable<RecipeIngredient> ingredients)
    {
        _ingredients.Clear();
        _ingredients.AddRange(ingredients);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceSteps(IEnumerable<RecipeStep> steps)
    {
        _steps.Clear();
        _steps.AddRange(steps);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceMediaAssets(IEnumerable<RecipeMediaAsset> mediaAssets)
    {
        _mediaAssets.Clear();
        _mediaAssets.AddRange(mediaAssets);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMediaAsset(RecipeMediaAsset mediaAsset)
    {
        _mediaAssets.Add(mediaAsset);
        UpdatedAt = DateTime.UtcNow;
    }
}
