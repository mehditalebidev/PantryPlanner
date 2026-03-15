using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.Users;
using PantryPlanner.Api.UnitTests.Support;

namespace PantryPlanner.Api.UnitTests.Features.Users.Signup;

public sealed class SignupHandlerTests
{
    [Fact]
    public async Task Handle_CreatesUserAndReturnsAuthResponse_WhenEmailIsAvailable()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new Repository(dbContext);
        var ingredientCatalogSeeder = new IngredientCatalogSeeder(dbContext);
        var passwordService = new FakePasswordService();
        var tokenService = new FakeTokenService();
        var handler = new SignupHandler(repository, ingredientCatalogSeeder, passwordService, tokenService);

        var result = await handler.Handle(new SignupCommand
        {
            Email = "  Mehdi@Example.com ",
            DisplayName = " Mehdi ",
            Password = "Password123!"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("mehdi@example.com", result.Value.User.Email);
        Assert.Equal("Mehdi", result.Value.User.DisplayName);
        Assert.Equal(FakeTokenService.FixedExpiresAt, result.Value.ExpiresAt);
        Assert.StartsWith("token-for-", result.Value.AccessToken, StringComparison.Ordinal);

        var savedUser = await dbContext.Users.SingleAsync();
        Assert.Equal("mehdi@example.com", savedUser.Email);
        Assert.Equal("Mehdi", savedUser.DisplayName);
        Assert.Equal("hashed::Password123!", savedUser.PasswordHash);
        Assert.True(await dbContext.Ingredients.AnyAsync(ingredient => ingredient.UserId == savedUser.Id));
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenEmailIsAlreadyInUse()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var existingUser = User.Create("mehdi@example.com", "Existing User");
        existingUser.SetPasswordHash("hashed::Password123!");
        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var repository = new Repository(dbContext);
        var ingredientCatalogSeeder = new IngredientCatalogSeeder(dbContext);
        var handler = new SignupHandler(repository, ingredientCatalogSeeder, new FakePasswordService(), new FakeTokenService());

        var result = await handler.Handle(new SignupCommand
        {
            Email = "MEHDI@EXAMPLE.COM",
            DisplayName = "Another User",
            Password = "Password123!"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("email_in_use", result.Error?.Code);
        Assert.Equal(1, await dbContext.Users.CountAsync());
    }
}
