namespace PantryPlanner.Api.Features.Users;

public sealed class User
{
    private User()
    {
    }

    private User(string email, string displayName)
    {
        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        DisplayName = displayName.Trim();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static User Create(string email, string displayName)
    {
        return new User(email, displayName);
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
