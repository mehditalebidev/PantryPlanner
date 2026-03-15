namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeStep
{
    private readonly List<RecipeStepIngredientReference> _ingredientReferences = [];

    private RecipeStep()
    {
    }

    private RecipeStep(string instruction, int sortOrder, int? durationMinutes)
    {
        Id = Guid.NewGuid();
        Instruction = instruction.Trim();
        SortOrder = sortOrder;
        DurationMinutes = durationMinutes;
    }

    public Guid Id { get; private set; }

    public Guid RecipeId { get; private set; }

    public Recipe Recipe { get; private set; } = null!;

    public string Instruction { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public int? DurationMinutes { get; private set; }

    public IReadOnlyCollection<RecipeStepIngredientReference> IngredientReferences => _ingredientReferences;

    public static RecipeStep Create(
        string instruction,
        int sortOrder,
        int? durationMinutes,
        IEnumerable<RecipeIngredient> referencedIngredients)
    {
        var step = new RecipeStep(instruction, sortOrder, durationMinutes);

        foreach (var ingredient in referencedIngredients.DistinctBy(ingredient => ingredient.Id))
        {
            step._ingredientReferences.Add(RecipeStepIngredientReference.Create(ingredient));
        }

        return step;
    }
}
