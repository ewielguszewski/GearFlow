using GearFlow.Modules.Catalog.Domain.Entities;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GearFlow.Shared.Infrastructure.Postgres.Configurations;

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

        builder.OwnsOne(x => x.BasePrice, b =>
            b.ConfigureMoney("base_price")
        );
        builder.Navigation(x => x.BasePrice)
            .IsRequired(false);

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();
    }
}
