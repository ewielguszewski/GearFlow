using GearFlow.Modules.Rentals.Domain.Entities;
using GearFlow.Shared.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Modules.Rentals.Infrastructure.DAL.Configurations;

public sealed class RentalLineConfiguration : IEntityTypeConfiguration<RentalLine>
{
    public void Configure(EntityTypeBuilder<RentalLine> builder)
    {
        builder.ToTable("RentalLines", "rentals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RentalId)
            .IsRequired();

        builder.Property(x => x.ReservationLineId)
            .IsRequired();

        builder.HasIndex(x => new { x.RentalId, x.ReservationLineId })
            .IsUnique();

        builder.ComplexProperty(x => x.Item, snapshot =>
        {
            snapshot.Property(x => x.ItemId).IsRequired();
            snapshot.Property(x => x.VariantId).IsRequired();
            snapshot.Property(x => x.VariantName);
            snapshot.Property(x => x.Brand).IsRequired();
            snapshot.Property(x => x.Model).IsRequired();
            snapshot.Property(x => x.PublicNote);
            snapshot.ComplexProperty(x => x.UnitPrice).ConfigureMoney("unit_price").IsRequired();
            snapshot.Property(x => x.PriceSource).HasConversion<string>().IsRequired();
            snapshot.Property(x => x.Size);
        });

        builder.ComplexProperty(x => x.LineTotalPrice)
            .ConfigureMoney("line_total_price")
            .IsRequired();

        builder.Property(x => x.PickupCondition)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.PickupConditionNote);

        builder.Property(x => x.PickupConditionRecordedAt)
            .IsRequired();

        builder.Property(x => x.ReturnCondition)
            .HasConversion<string?>();

        builder.Property(x => x.ReturnConditionNote);
        builder.Property(x => x.ReturnConditionRecordedAt);

        builder.ComplexProperty(x => x.DamageFee)
            .ConfigureMoney("damage_fee")
            .IsRequired();
    }
}
