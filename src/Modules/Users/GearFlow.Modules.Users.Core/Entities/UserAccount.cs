using GearFlow.Modules.Users.Core.Enums;

namespace GearFlow.Modules.Users.Core.Entities;

public class UserAccount
{
    private readonly List<RefreshToken> _refreshTokens = new();

    public Guid Id { get; private set; }

    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public Role Role { get; private set; }
    public DateTime CreatedAt { get; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;


    private UserAccount() { } // EF

    public UserAccount(string email, string passwordHash, Role role, DateTime utcNow)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = utcNow;
    }

    public void AttachCustomer(Customer customer)
    {
        Customer = customer;
        CustomerId = customer.Id;
    }
}
