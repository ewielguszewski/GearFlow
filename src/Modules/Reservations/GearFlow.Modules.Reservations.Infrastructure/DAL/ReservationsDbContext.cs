using GearFlow.Modules.Reservations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Reservations.Infrastructure.DAL;

public sealed class ReservationsDbContext : DbContext
{
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationLine> ReservationLines { get; set; }

    public ReservationsDbContext(DbContextOptions<ReservationsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reservations");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
