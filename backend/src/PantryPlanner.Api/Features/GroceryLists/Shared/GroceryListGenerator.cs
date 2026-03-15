using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.MealPlans;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GroceryListGenerator : IGroceryListGenerator
{
    private readonly IRepository _repository;

    public GroceryListGenerator(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GroceryList>> GenerateAsync(Guid userId, Guid mealPlanId, CancellationToken cancellationToken)
    {
        var mealPlan = await _repository.Query<MealPlan>()
            .Where(plan => plan.UserId == userId && plan.Id == mealPlanId)
            .Include(plan => plan.Entries)
                .ThenInclude(entry => entry.Recipe)
                    .ThenInclude(recipe => recipe.Ingredients)
                        .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .SingleOrDefaultAsync(cancellationToken);

        if (mealPlan is null)
        {
            return Result<GroceryList>.Failure(GroceryListErrors.MealPlanNotFound(mealPlanId));
        }

        var aggregates = new Dictionary<string, GroceryItemAggregate>(StringComparer.Ordinal);

        foreach (var entry in mealPlan.Entries)
        {
            var servingsFactor = GetServingsFactor(entry);

            foreach (var recipeIngredient in entry.Recipe.Ingredients)
            {
                var aggregateKey = BuildAggregateKey(recipeIngredient);
                var unitCode = recipeIngredient.NormalizedQuantity.HasValue && !string.IsNullOrWhiteSpace(recipeIngredient.NormalizedUnitCode)
                    ? recipeIngredient.NormalizedUnitCode!
                    : recipeIngredient.UnitCode;
                var quantity = recipeIngredient.NormalizedQuantity.HasValue && !string.IsNullOrWhiteSpace(recipeIngredient.NormalizedUnitCode)
                    ? recipeIngredient.NormalizedQuantity.Value * servingsFactor
                    : recipeIngredient.Quantity * servingsFactor;

                if (!aggregates.TryGetValue(aggregateKey, out var aggregate))
                {
                    aggregate = new GroceryItemAggregate(
                        recipeIngredient.IngredientId,
                        recipeIngredient.Ingredient.Name,
                        unitCode);
                    aggregates.Add(aggregateKey, aggregate);
                }

                aggregate.Add(quantity);
            }
        }

        var groceryList = GroceryList.Create(userId, mealPlan.Id, mealPlan.StartDate, mealPlan.EndDate);
        groceryList.ReplaceItems(aggregates.Values
            .OrderBy(item => item.Name)
            .ThenBy(item => item.UnitCode)
            .Select(item => GroceryListItem.Create(item.IngredientId, item.Name, item.Quantity, item.UnitCode, item.SourceCount))
            .ToArray());

        return Result<GroceryList>.Success(groceryList);
    }

    private static decimal GetServingsFactor(PlannedMeal entry)
    {
        if (!entry.ServingsOverride.HasValue)
        {
            return 1m;
        }

        return decimal.Round((decimal)entry.ServingsOverride.Value / entry.Recipe.Servings, 6, MidpointRounding.AwayFromZero);
    }

    private static string BuildAggregateKey(RecipeIngredient recipeIngredient)
    {
        if (recipeIngredient.NormalizedQuantity.HasValue && !string.IsNullOrWhiteSpace(recipeIngredient.NormalizedUnitCode))
        {
            return $"{recipeIngredient.IngredientId:N}|normalized|{recipeIngredient.NormalizedUnitCode}";
        }

        return $"{recipeIngredient.IngredientId:N}|authored|{recipeIngredient.UnitCode}";
    }

    private sealed class GroceryItemAggregate
    {
        public GroceryItemAggregate(Guid ingredientId, string name, string unitCode)
        {
            IngredientId = ingredientId;
            Name = name;
            UnitCode = unitCode;
        }

        public Guid IngredientId { get; }

        public string Name { get; }

        public string UnitCode { get; }

        public decimal Quantity { get; private set; }

        public int SourceCount { get; private set; }

        public void Add(decimal quantity)
        {
            Quantity = decimal.Round(Quantity + quantity, 6, MidpointRounding.AwayFromZero);
            SourceCount++;
        }
    }
}
