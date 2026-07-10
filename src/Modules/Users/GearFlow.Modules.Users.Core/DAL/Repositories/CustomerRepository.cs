using GearFlow.Modules.Users.Core.Entities;
using GearFlow.Modules.Users.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Users.Core.DAL.Repositories;

internal class CustomerRepository : ICustomerRepository
{
    private readonly UsersDbContext _dbContext;
    private readonly DbSet<Customer> _customer;

    public CustomerRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
        _customer = dbContext.Customers;
    }

    public void Add(Customer customer)
        => _customer.Add(customer);

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => _customer.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
}
