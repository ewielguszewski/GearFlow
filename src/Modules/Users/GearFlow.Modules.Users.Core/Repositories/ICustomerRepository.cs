using GearFlow.Modules.Users.Core.Entities;

namespace GearFlow.Modules.Users.Core.Repositories;

public interface ICustomerRepository
{
    void Add(Customer customer);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}
