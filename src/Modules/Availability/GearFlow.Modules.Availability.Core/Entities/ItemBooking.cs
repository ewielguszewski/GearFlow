using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Availability.Core.Entities;

public class ItemBooking
{
    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid VariantId { get; private set; }
    public DateRange TimePeriod { get; private set; }
    public Guid SourceId { get; private set; }
    public BookingType Source { get; private set; }
    
    private ItemBooking() { } // EF

    private ItemBooking(Guid itemId, Guid variantId, DateRange timePeriod, Guid sourceId, BookingType source)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        VariantId = variantId;
        TimePeriod = timePeriod;
        SourceId = sourceId;
        Source = source;
    }

    public static ItemBooking Create(Guid itemId, Guid variantId, DateRange timePeriod, Guid sourceId, BookingType source)
        => new ItemBooking(itemId, variantId, timePeriod, sourceId, source);
}


public enum BookingType
{
    Reservation,
    Maintenance,
    Rental,
    ManualBlock
}

// for now just delete record if not affecting availability anymore (no BookingStatus)

// future: need to block daterange.start or daterange.end on days like sundays and overall maybe some calendar in future to
// manage when rental office is open? in that case need to have like schedule of how far in the future can be booked