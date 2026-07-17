using GearFlow.Modules.Rentals.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Rentals.Infrastructure.DAL;

public sealed class RentalsDbContext : DbContext
{
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<RentalLine> RentalLines { get; set; }

    public RentalsDbContext(DbContextOptions<RentalsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rentals");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
