namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeStepIngredientReference
{
    private RecipeStepIngredientReference()
    {
    }

    private RecipeStepIngredientReference(RecipeIngredient recipeIngredient)
    {
        Id = Guid.NewGuid();
        RecipeIngredient = recipeIngredient;
        RecipeIngredientId = recipeIngredient.Id;
    }

    public Guid Id { get; private set; }

    public Guid RecipeStepId { get; private set; }

    public RecipeStep RecipeStep { get; private set; } = null!;

    public Guid RecipeIngredientId { get; private set; }

    public RecipeIngredient RecipeIngredient { get; private set; } = null!;

    public static RecipeStepIngredientReference Create(RecipeIngredient recipeIngredient)
    {
        return new RecipeStepIngredientReference(recipeIngredient);
    }
}
