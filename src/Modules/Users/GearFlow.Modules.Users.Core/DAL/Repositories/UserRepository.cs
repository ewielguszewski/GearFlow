using GearFlow.Modules.Users.Core.Entities;
using GearFlow.Modules.Users.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Users.Core.DAL.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly UsersDbContext _dbContext;
    private readonly DbSet<UserAccount> _users;

    public UserRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
        _users = dbContext.Users;
    }

    public void Add(UserAccount user)
        => _users.Add(user);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        => _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken)
       => _users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        => _users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
}
