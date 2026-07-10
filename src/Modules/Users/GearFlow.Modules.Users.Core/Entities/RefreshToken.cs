namespace GearFlow.Modules.Users.Core.Entities;

public class RefreshToken
{
    public Guid Id { get; init; }
    public string Token { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; init; }

    public Guid UserId { get; init; }
    public UserAccount User { get; init; } = default!;

    private RefreshToken() { }

    private RefreshToken(string token, DateTime expiresAt, DateTime createdAt, Guid userId)
    {
        Id = Guid.NewGuid();
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        UserId = userId;
    }

    public static RefreshToken Create(string token, DateTime expiresAt, DateTime createdAt, Guid userId)
        => new(token, expiresAt, createdAt, userId);

    public bool IsExpired(DateTime utcNow)
        => ExpiresAt <= utcNow;

    public void Revoke()
        => IsRevoked = true;
}
