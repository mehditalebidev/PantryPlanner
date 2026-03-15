using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Users;
using PantryPlanner.Api.UnitTests.Support;

namespace PantryPlanner.Api.UnitTests.Features.Users.Login;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAuthResponse_WhenCredentialsAreValid()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var passwordService = new FakePasswordService();
        var user = User.Create("mehdi@example.com", "Mehdi");
        user.SetPasswordHash(passwordService.HashPassword(user, "Password123!"));
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var repository = new Repository(dbContext);
        var handler = new LoginHandler(repository, passwordService, new FakeTokenService());

        var result = await handler.Handle(new LoginCommand
        {
            Email = " MEHDI@EXAMPLE.COM ",
            Password = "Password123!"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.User.Id);
        Assert.Equal("mehdi@example.com", result.Value.User.Email);
        Assert.Equal(FakeTokenService.FixedExpiresAt, result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new Repository(dbContext);
        var handler = new LoginHandler(repository, new FakePasswordService(), new FakeTokenService());

        var result = await handler.Handle(new LoginCommand
        {
            Email = "missing@example.com",
            Password = "Password123!"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("invalid_credentials", result.Error?.Code);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenPasswordIsInvalid()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var passwordService = new FakePasswordService();
        var user = User.Create("mehdi@example.com", "Mehdi");
        user.SetPasswordHash(passwordService.HashPassword(user, "Password123!"));
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var repository = new Repository(dbContext);
        var handler = new LoginHandler(repository, passwordService, new FakeTokenService());

        var result = await handler.Handle(new LoginCommand
        {
            Email = "mehdi@example.com",
            Password = "WrongPassword123!"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("invalid_credentials", result.Error?.Code);
    }
}
