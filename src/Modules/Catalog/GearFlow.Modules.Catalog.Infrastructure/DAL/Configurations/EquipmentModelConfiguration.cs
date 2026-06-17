using GearFlow.Modules.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Modules.Catalog.Infrastructure.DAL.Configurations;

public sealed class EquipmentModelConfiguration : IEntityTypeConfiguration<EquipmentModel>
{
    public void Configure(EntityTypeBuilder<EquipmentModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Brand)
            .IsRequired();

        builder.Property(x => x.Model)
            .IsRequired();

        builder.Property(x => x.Slug)
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property<decimal?>("_basePriceAmount")
            .HasColumnName("base_price_amount")
            .HasPrecision(18, 2);

        builder.Property<string?>("_basePriceCurrency")
            .HasColumnType("base_price_currency")
            .HasMaxLength(3);

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();
    }
}
