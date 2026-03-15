namespace PantryPlanner.Api.IntegrationTests;

public sealed record TestUserData(string Email, string DisplayName, string Password)
{
    public static TestUserData Seeded { get; } = new(
        "seeded.user@example.com",
        "Seeded User",
        "Password123!");

    public static TestUserData NewUser(string prefix = "user")
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        return new TestUserData(
            $"{prefix}-{uniqueId}@example.com",
            $"{prefix} user",
            "Password123!");
    }
}
