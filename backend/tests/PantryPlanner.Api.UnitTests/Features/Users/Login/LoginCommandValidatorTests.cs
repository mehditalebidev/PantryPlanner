using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Features.Users.Login;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ReturnsNoErrors_ForValidCommand()
    {
        var command = new LoginCommand
        {
            Email = "mehdi@example.com",
            Password = "Password123!"
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ReturnsExpectedErrors_ForInvalidCommand()
    {
        var command = new LoginCommand
        {
            Email = string.Empty,
            Password = "short"
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Email is required.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Password must be between 8 and 100 characters.");
    }
}
