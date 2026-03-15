using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Support;

internal sealed class FakeTokenService : ITokenService
{
    public static readonly DateTime FixedExpiresAt = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public IssuedToken CreateAccessToken(User user)
    {
        return new IssuedToken($"token-for-{user.Id}", FixedExpiresAt);
    }
}
