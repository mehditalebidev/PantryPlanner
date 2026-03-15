using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Features.Users.Signup;

public sealed class SignupCommandValidatorTests
{
    private readonly SignupCommandValidator _validator = new();

    [Fact]
    public void Validate_ReturnsNoErrors_ForValidCommand()
    {
        var command = new SignupCommand
        {
            Email = "mehdi@example.com",
            DisplayName = "Mehdi",
            Password = "Password123!"
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ReturnsExpectedErrors_ForInvalidCommand()
    {
        var command = new SignupCommand
        {
            Email = "invalid-email",
            DisplayName = "M",
            Password = "short"
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Email must be a valid email address.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "DisplayName must be between 2 and 100 characters.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Password must be between 8 and 100 characters.");
    }
}
