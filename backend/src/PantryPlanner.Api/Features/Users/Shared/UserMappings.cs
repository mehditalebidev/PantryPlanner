namespace PantryPlanner.Api.Features.Users;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(user.Id, user.Email, user.DisplayName);
    }
}
