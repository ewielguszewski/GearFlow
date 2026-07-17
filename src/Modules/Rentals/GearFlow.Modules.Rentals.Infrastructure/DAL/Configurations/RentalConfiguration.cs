using GearFlow.Modules.Rentals.Domain.Entities;
using GearFlow.Shared.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Modules.Rentals.Infrastructure.DAL.Configurations;

public sealed class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.ToTable("Rentals", "rentals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasIndex(x => x.ReservationId)
            .IsUnique();

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.RentalLines)
            .WithOne(x => x.Rental)
            .HasForeignKey(x => x.RentalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.RentalLines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.ReservationId)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.ComplexProperty(x => x.RentalPeriod)
            .ConfigureDateRange("rental_period")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.PickedUpAt)
            .IsRequired();

        builder.Property(x => x.ReturnedAt);
        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.Currency)
            .ConfigureCurrencyCode("currency");

        builder.ComplexProperty(x => x.TotalPrice)
            .ConfigureMoney("total_price")
            .IsRequired();

        builder.ComplexProperty(x => x.LateFee)
            .ConfigureMoney("late_fee")
            .IsRequired();
    }
}
