using GearFlow.Modules.Users.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace GearFlow.Modules.Users.Core.Security;

public class PasswordManager : IPasswordManager
{
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public PasswordManager(IPasswordHasher<UserAccount> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string Secure(string password)
        => _passwordHasher.HashPassword(null!, password);

    public bool Validate(string password, string securedPassword)
        => _passwordHasher.VerifyHashedPassword(null!, securedPassword, password) ==
            PasswordVerificationResult.Success;
}
