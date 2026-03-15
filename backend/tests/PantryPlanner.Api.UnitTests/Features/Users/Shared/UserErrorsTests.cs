using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Features.Users.Shared;

public sealed class UserErrorsTests
{
    [Fact]
    public void EmailInUse_ReturnsConflictError()
    {
        var error = UserErrors.EmailInUse();

        Assert.Equal("email_in_use", error.Code);
        Assert.Equal(409, error.StatusCode);
    }

    [Fact]
    public void InvalidCredentials_ReturnsUnauthorizedError()
    {
        var error = UserErrors.InvalidCredentials();

        Assert.Equal("invalid_credentials", error.Code);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    public void NotFound_ReturnsNotFoundError()
    {
        var error = UserErrors.NotFound();

        Assert.Equal("user_not_found", error.Code);
        Assert.Equal(404, error.StatusCode);
    }
}
