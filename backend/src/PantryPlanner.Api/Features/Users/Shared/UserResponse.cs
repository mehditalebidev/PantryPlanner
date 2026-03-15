namespace PantryPlanner.Api.Features.Users;

public sealed record UserResponse(Guid Id, string Email, string DisplayName);
