using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Modules.Reservations.Infrastructure.DAL.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasMany(x => x.ReservationLines)
            .WithOne(x => x.Reservation)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.ReservationLines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.HasIndex(x => x.CustomerId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Draft'");

        builder.ComplexProperty(x => x.ReservedPeriod)
            .ConfigureDateRange("reserved_period")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CancReason)
            .HasConversion<string?>();

        builder.Property(x => x.SelectedPaymentMethod)
            .HasConversion<string?>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.TtlExpiresAt)
            .IsRequired();
        
        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.Currency)
            .ConfigureCurrencyCode("currency");

        builder.ComplexProperty(x => x.PaidAmount)
            .ConfigureMoney("paid_amount")
            .IsRequired();
        
        builder.ComplexProperty(x => x.TotalPrice)
            .ConfigureMoney("total_price")
            .IsRequired();
    }
}
