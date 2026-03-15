using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.MealPlans;
using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.Features.Users;
using PantryPlanner.Api.Features.GroceryLists;

namespace PantryPlanner.Api.Common.Persistence;

public sealed class PantryPlannerDbContext : DbContext
{
    public PantryPlannerDbContext(DbContextOptions<PantryPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

    public DbSet<RecipeStepIngredientReference> RecipeStepIngredientReferences => Set<RecipeStepIngredientReference>();

    public DbSet<RecipeMediaAsset> RecipeMediaAssets => Set<RecipeMediaAsset>();

    public DbSet<MealPlan> MealPlans => Set<MealPlan>();

    public DbSet<MealSlot> MealSlots => Set<MealSlot>();

    public DbSet<PlannedMeal> PlannedMeals => Set<PlannedMeal>();

    public DbSet<GroceryList> GroceryLists => Set<GroceryList>();

    public DbSet<GroceryListItem> GroceryListItems => Set<GroceryListItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PantryPlannerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
