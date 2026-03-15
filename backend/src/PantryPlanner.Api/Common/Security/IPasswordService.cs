using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.Common.Security;

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string password);
}
