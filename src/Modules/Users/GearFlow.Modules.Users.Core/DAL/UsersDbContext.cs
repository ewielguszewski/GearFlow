using GearFlow.Modules.Users.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Users.Core.DAL;

public class UsersDbContext : DbContext
{
    public DbSet<UserAccount> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
