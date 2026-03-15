using System.Net;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Users;

public static class UserErrors
{
    public static Error EmailInUse()
    {
        return new Error(
            "email_in_use",
            "Email is already in use.",
            "A user with that email already exists.",
            (int)HttpStatusCode.Conflict);
    }

    public static Error InvalidCredentials()
    {
        return new Error(
            "invalid_credentials",
            "Credentials are invalid.",
            "Email or password is incorrect.",
            (int)HttpStatusCode.Unauthorized);
    }

    public static Error NotFound()
    {
        return new Error(
            "user_not_found",
            "User was not found.",
            "The requested user does not exist.",
            (int)HttpStatusCode.NotFound);
    }
}
