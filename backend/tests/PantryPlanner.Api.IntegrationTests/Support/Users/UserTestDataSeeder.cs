using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.IntegrationTests;

public static class UserTestDataSeeder
{
    public static async Task SeedBaselineAsync(IServiceProvider services)
    {
        await EnsureUserExistsAsync(services, TestUserData.Seeded);
    }

    public static async Task<User> EnsureUserExistsAsync(IServiceProvider services, TestUserData testUser)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PantryPlannerDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        var existingUser = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Email == testUser.Email);

        if (existingUser is not null)
        {
            return existingUser;
        }

        var user = User.Create(testUser.Email, testUser.DisplayName);
        user.SetPasswordHash(passwordService.HashPassword(user, testUser.Password));

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var ingredientCatalogSeeder = scope.ServiceProvider.GetRequiredService<IIngredientCatalogSeeder>();
        await ingredientCatalogSeeder.SeedDefaultsForUserAsync(user.Id, CancellationToken.None);

        return user;
    }
}
