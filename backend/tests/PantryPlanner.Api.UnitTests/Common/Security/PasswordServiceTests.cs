using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Common.Security;

public sealed class PasswordServiceTests
{
    private readonly PasswordService _service = new();

    [Fact]
    public void HashPassword_ReturnsHashThatCanBeVerified()
    {
        var user = User.Create("mehdi@example.com", "Mehdi");

        var hash = _service.HashPassword(user, "Password123!");
        user.SetPasswordHash(hash);

        Assert.NotEqual("Password123!", hash);
        Assert.True(_service.VerifyPassword(user, "Password123!"));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_WhenPasswordDoesNotMatch()
    {
        var user = User.Create("mehdi@example.com", "Mehdi");
        user.SetPasswordHash(_service.HashPassword(user, "Password123!"));

        var isValid = _service.VerifyPassword(user, "WrongPassword123!");

        Assert.False(isValid);
    }
}
