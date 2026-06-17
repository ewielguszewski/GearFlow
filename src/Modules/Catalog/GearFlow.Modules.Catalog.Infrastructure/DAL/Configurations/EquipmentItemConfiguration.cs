using GearFlow.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Catalog.Infrastructure.DAL.Configurations;

public class EquipmentItemConfiguration : IEntityTypeConfiguration<EquipmentItem>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<EquipmentItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EquipmentVariantId)
            .IsRequired();

        builder.Property(x => x.AssetTag)
            .IsRequired();

        builder.Property(x => x.InternalNote);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.LastMaintenanceAt);

        builder.Property(x => x.NextInspectionAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
