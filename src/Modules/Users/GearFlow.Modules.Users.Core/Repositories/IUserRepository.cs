using GearFlow.Modules.Users.Core.Entities;

namespace GearFlow.Modules.Users.Core.Repositories;

public interface IUserRepository
{
    void Add(UserAccount user);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}
