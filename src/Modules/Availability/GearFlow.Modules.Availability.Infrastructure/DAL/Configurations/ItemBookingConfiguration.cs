using GearFlow.Modules.Availability.Core.Entities;
using GearFlow.Shared.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Modules.Availability.Infrastructure.DAL.Configurations;

public sealed class ItemBookingConfiguration : IEntityTypeConfiguration<ItemBooking>
{
    public void Configure(EntityTypeBuilder<ItemBooking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemId)
            .IsRequired();
        
        builder.Property(x => x.VariantId)
            .IsRequired();
        
        builder.ComplexProperty(x => x.TimePeriod)
            .ConfigureDateRange("time_period")
            .IsRequired();
        
        builder.Property(x => x.SourceId)
            .IsRequired();
        
        builder.Property(x => x.Source)
            .HasConversion<string>()
            .IsRequired();
    }
}