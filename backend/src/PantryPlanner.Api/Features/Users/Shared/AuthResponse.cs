namespace PantryPlanner.Api.Features.Users;

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);
