using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Modules.Reservations.Infrastructure.DAL.Configurations;

public sealed class ReservationLineConfiguration : IEntityTypeConfiguration<ReservationLine>
{
    public void Configure(EntityTypeBuilder<ReservationLine> builder)
    {
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservationId)
            .IsRequired();

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
    }
}