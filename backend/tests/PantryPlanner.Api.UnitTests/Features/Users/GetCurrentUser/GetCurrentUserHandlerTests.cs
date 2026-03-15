using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Users;
using PantryPlanner.Api.UnitTests.Support;

namespace PantryPlanner.Api.UnitTests.Features.Users.GetCurrentUser;

public sealed class GetCurrentUserHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsUserResponse_WhenUserExists()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var user = User.Create("mehdi@example.com", "Mehdi");
        user.SetPasswordHash("hashed::Password123!");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var repository = new Repository(dbContext);
        var handler = new GetCurrentUserHandler(repository);

        var result = await handler.Handle(new GetCurrentUserQuery(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(user.Email, result.Value.Email);
        Assert.Equal(user.DisplayName, result.Value.DisplayName);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new Repository(dbContext);
        var handler = new GetCurrentUserHandler(repository);

        var result = await handler.Handle(new GetCurrentUserQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user_not_found", result.Error?.Code);
    }
}
