using GearFlow.Modules.Availability.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Availability.Infrastructure.DAL;

public sealed class AvailabilityDbContext : DbContext
{
    public DbSet<ItemBooking> Bookings { get; set; }

    public AvailabilityDbContext(DbContextOptions<AvailabilityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("availability");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
