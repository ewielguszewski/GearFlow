using GearFlow.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Catalog.Infrastructure.DAL;

public sealed class CatalogDbContext : DbContext
{
    public DbSet<EquipmentModel> EquipmentModels { get; set; }
    public DbSet<EquipmentVariant> EquipmentVariants { get; set; }
    public DbSet<EquipmentItem> EquipmentItems { get; set; }

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
