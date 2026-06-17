using GearFlow.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GearFlow.Shared.Infrastructure.Postgres.Configurations;

namespace GearFlow.Modules.Catalog.Infrastructure.DAL.Configurations;

public class EquipmentVariantConfiguration : IEntityTypeConfiguration<EquipmentVariant>
{
    public void Configure(EntityTypeBuilder<EquipmentVariant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EquipmentModelId)
            .IsRequired();

        builder.Property(x => x.DisplayName);

        builder.Property(x => x.PublicNote);

        builder.Property<decimal?>("_overriddenPriceAmount")
            .HasColumnName("overridden_price_amount")
            .HasPrecision(18, 2);

        builder.Property<string?>("_overriddenPriceCurrency")
            .HasColumnName("overridden_price_currency")
            .HasMaxLength(3);

        builder.Property(x => x.Size);
    }
}