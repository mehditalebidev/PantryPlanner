namespace PantryPlanner.Api.Common.Security;

public sealed record IssuedToken(string AccessToken, DateTime ExpiresAt);
