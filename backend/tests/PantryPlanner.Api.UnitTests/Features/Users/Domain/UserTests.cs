using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Features.Users.Domain;

public sealed class UserTests
{
    [Fact]
    public void Create_NormalizesEmailAndTrimsDisplayName()
    {
        var before = DateTime.UtcNow;

        var user = User.Create("  Mehdi@Example.COM ", "  Mehdi Talebi  ");

        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("mehdi@example.com", user.Email);
        Assert.Equal("Mehdi Talebi", user.DisplayName);
        Assert.InRange(user.CreatedAt, before, after);
        Assert.InRange(user.UpdatedAt, before, after);
    }

    [Fact]
    public void SetPasswordHash_UpdatesHashAndTimestamp()
    {
        var user = User.Create("mehdi@example.com", "Mehdi");
        var originalUpdatedAt = user.UpdatedAt;

        Thread.Sleep(5);
        user.SetPasswordHash("hashed-password");

        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.True(user.UpdatedAt > originalUpdatedAt);
    }

    [Theory]
    [InlineData("  Mehdi@Example.COM  ", "mehdi@example.com")]
    [InlineData("USER@EXAMPLE.COM", "user@example.com")]
    public void NormalizeEmail_TrimsAndLowercases(string email, string expected)
    {
        var normalized = User.NormalizeEmail(email);

        Assert.Equal(expected, normalized);
    }
}
