using PantryPlanner.Api.Common.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PantryPlanner.Api.UnitTests.Common.Security;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetRequiredUserId_ReturnsSubClaim_WhenPresent()
    {
        var userId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]));

        var result = principal.GetRequiredUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetRequiredUserId_FallsBackToNameIdentifierClaim()
    {
        var userId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));

        var result = principal.GetRequiredUserId();

        Assert.Equal(userId, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    public void GetRequiredUserId_Throws_WhenClaimIsMissingOrInvalid(string? claimValue)
    {
        var claims = claimValue is null
            ? Array.Empty<Claim>()
            : [new Claim(JwtRegisteredClaimNames.Sub, claimValue)];
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var exception = Assert.Throws<UnauthorizedAccessException>(() => principal.GetRequiredUserId());

        Assert.Equal("Authenticated user id claim is missing.", exception.Message);
    }
}
