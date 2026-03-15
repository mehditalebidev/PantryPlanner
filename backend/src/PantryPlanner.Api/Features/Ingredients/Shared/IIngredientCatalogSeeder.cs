namespace PantryPlanner.Api.Features.Ingredients;

public interface IIngredientCatalogSeeder
{
    Task SeedDefaultsForExistingUsersAsync(CancellationToken cancellationToken);

    Task SeedDefaultsForUserAsync(Guid userId, CancellationToken cancellationToken);
}
