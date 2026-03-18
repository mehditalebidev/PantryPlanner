using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class MealPlanContentFactory : IMealPlanContentFactory
{
    private readonly PantryPlannerDbContext _dbContext;

    public MealPlanContentFactory(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<MealPlanContentDraft>> BuildAsync(
        Guid userId,
        IReadOnlyCollection<MealSlotWriteModel> slots,
        IReadOnlyCollection<PlannedMealWriteModel> entries,
        CancellationToken cancellationToken)
    {
        var recipeIds = entries
            .Select(entry => entry.RecipeId)
            .Distinct()
            .ToArray();

        var recipesById = await _dbContext.Set<Recipe>()
            .Where(recipe => recipe.UserId == userId && recipeIds.Contains(recipe.Id))
            .ToDictionaryAsync(recipe => recipe.Id, cancellationToken);

        if (recipesById.Count != recipeIds.Length)
        {
            return Result<MealPlanContentDraft>.Failure(MealPlanErrors.InvalidRecipeReference());
        }

        var slotMap = slots
            .Select(slot => MealSlot.Create(slot.ReferenceKey, slot.Name, slot.SortOrder, slot.IsDefault))
            .ToDictionary(slot => slot.ReferenceKey, StringComparer.OrdinalIgnoreCase);

        var plannedMeals = new List<PlannedMeal>(entries.Count);

        foreach (var entry in entries)
        {
            if (!slotMap.TryGetValue(entry.MealSlotReferenceKey.Trim(), out var slot))
            {
                return Result<MealPlanContentDraft>.Failure(MealPlanErrors.InvalidSlotReference());
            }

            var recipe = recipesById[entry.RecipeId];

            plannedMeals.Add(PlannedMeal.Create(slot, recipe, entry.PlannedDate, entry.ServingsOverride, entry.Note));
        }

        return Result<MealPlanContentDraft>.Success(new MealPlanContentDraft(slotMap.Values.ToArray(), plannedMeals));
    }
}
