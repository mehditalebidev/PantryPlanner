using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class IngredientCatalogSeeder : IIngredientCatalogSeeder
{
    private readonly PantryPlannerDbContext _dbContext;

    public IngredientCatalogSeeder(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedDefaultsForExistingUsersAsync(CancellationToken cancellationToken)
    {
        var userIds = await _dbContext.Users
            .AsNoTracking()
            .Select(user => user.Id)
            .ToArrayAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            await SeedDefaultsForUserAsync(userId, cancellationToken);
        }
    }

    public async Task SeedDefaultsForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var existingNormalizedNames = await _dbContext.Ingredients
            .Where(ingredient => ingredient.UserId == userId)
            .Select(ingredient => ingredient.NormalizedName)
            .ToListAsync(cancellationToken);

        var existingLookup = existingNormalizedNames.ToHashSet(StringComparer.Ordinal);
        var missingIngredients = DefaultIngredientCatalog.All
            .Where(name => !existingLookup.Contains(Ingredient.NormalizeName(name)))
            .Select(name => Ingredient.Create(userId, name))
            .ToArray();

        if (missingIngredients.Length == 0)
        {
            return;
        }

        await _dbContext.Ingredients.AddRangeAsync(missingIngredients, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
