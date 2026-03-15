using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.GroceryLists;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.MealPlans;
using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.UnitTests.Support;

namespace PantryPlanner.Api.UnitTests.Features.GroceryLists.Shared;

public sealed class GroceryListGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_AggregatesNormalizedAndAuthoredQuantities_WithServingsOverrides()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new Repository(dbContext);
        var generator = new GroceryListGenerator(repository);
        var userId = Guid.NewGuid();

        var chicken = Ingredient.Create(userId, "Chicken thighs");
        var salt = Ingredient.Create(userId, "Salt");
        dbContext.Ingredients.AddRange(chicken, salt);

        var recipe = Recipe.Create(userId, "Chicken Dinner", null, 4, null, null, null);
        recipe.ReplaceIngredients(
        [
            RecipeIngredient.Create(chicken, "chicken", 2m, "lb", 907.18474m, "g", null, 1),
            RecipeIngredient.Create(salt, "salt", 1m, "pinch", null, null, null, 2)
        ]);
        recipe.ReplaceSteps(
        [
            RecipeStep.Create("Cook the chicken.", 1, null, recipe.Ingredients)
        ]);

        var mealPlan = MealPlan.Create(userId, "Week of March 16", new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 22));
        var dinnerSlot = MealSlot.Create("dinner", "Dinner", 1, true);
        mealPlan.ReplaceSlots([dinnerSlot]);
        mealPlan.ReplaceEntries(
        [
            PlannedMeal.Create(dinnerSlot, recipe, new DateOnly(2026, 3, 16), 2, null),
            PlannedMeal.Create(dinnerSlot, recipe, new DateOnly(2026, 3, 17), null, null)
        ]);

        dbContext.Recipes.Add(recipe);
        dbContext.MealPlans.Add(mealPlan);
        await dbContext.SaveChangesAsync();

        var result = await generator.GenerateAsync(userId, mealPlan.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var groceryList = result.Value;
        var chickenItem = Assert.Single(groceryList.Items, item => item.IngredientId == chicken.Id);
        var saltItem = Assert.Single(groceryList.Items, item => item.IngredientId == salt.Id);

        Assert.Equal(1360.77711m, chickenItem.Quantity);
        Assert.Equal("g", chickenItem.UnitCode);
        Assert.Equal(2, chickenItem.SourceCount);
        Assert.Equal(1.5m, saltItem.Quantity);
        Assert.Equal("pinch", saltItem.UnitCode);
        Assert.Equal(2, saltItem.SourceCount);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsNotFound_WhenMealPlanDoesNotExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new Repository(dbContext);
        var generator = new GroceryListGenerator(repository);

        var result = await generator.GenerateAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("meal_plan_not_found", result.Error?.Code);
    }
}
