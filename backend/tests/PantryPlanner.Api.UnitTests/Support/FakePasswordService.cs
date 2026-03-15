using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Support;

internal sealed class FakePasswordService : IPasswordService
{
    public string HashPassword(User user, string password)
    {
        return $"hashed::{password}";
    }

    public bool VerifyPassword(User user, string password)
    {
        return user.PasswordHash == HashPassword(user, password);
    }
}
