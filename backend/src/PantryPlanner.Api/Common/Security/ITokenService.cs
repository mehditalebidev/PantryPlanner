using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.Common.Security;

public interface ITokenService
{
    IssuedToken CreateAccessToken(User user);
}
