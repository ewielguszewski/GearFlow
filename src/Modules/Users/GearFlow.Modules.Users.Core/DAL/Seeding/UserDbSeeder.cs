using GearFlow.Modules.Users.Core.Entities;
using GearFlow.Shared.Abstractions.Enums;
using GearFlow.Modules.Users.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Users.Core.DAL.Seeding;

public sealed class UserDbSeeder
{
    private readonly UsersDbContext _dbContext;
    private readonly IPasswordManager _passwordManager;

    public UserDbSeeder(UsersDbContext dbContext, IPasswordManager passwordManager)
    {
        _dbContext = dbContext;
        _passwordManager = passwordManager;
    }

    public async Task SeedAsync()
    {
        if (await _dbContext.Users.AnyAsync())
            return;
        
        var now = DateTime.UtcNow;
        var passwordRaw = "password";
        var hashedPassword = _passwordManager.Secure(passwordRaw);

        var admin1 = new UserAccount("admin@admin.com", hashedPassword, Role.Admin, now);

        var employee1 = new UserAccount("employee@employee.com", hashedPassword, Role.Employee, now);

        await _dbContext.Users.AddRangeAsync(admin1, employee1);
        await _dbContext.SaveChangesAsync();
    }
}
