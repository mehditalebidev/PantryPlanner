using Microsoft.EntityFrameworkCore;

namespace PantryPlanner.Api.Features.MealPlans;

public static class MealPlanQueryExtensions
{
    public static IQueryable<MealPlan> IncludeMealPlanDetails(this IQueryable<MealPlan> query)
    {
        return query
            .Include(mealPlan => mealPlan.Slots)
            .Include(mealPlan => mealPlan.Entries)
                .ThenInclude(entry => entry.MealSlot)
            .Include(mealPlan => mealPlan.Entries)
                .ThenInclude(entry => entry.Recipe);
    }
}
