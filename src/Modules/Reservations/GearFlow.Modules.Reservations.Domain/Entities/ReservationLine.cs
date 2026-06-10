using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Domain.Entities;


// Snapshot of the selected catalog offer and concrete item.
public class ReservationLine
{
    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Reservation Reservation { get; private set; } = default!;

    // Stores the selected offer, variant, and held item without referencing live Catalog entities.
    public ReservedItemSnapshot Item { get; private set; } = default!;
    

    public Money LineTotalPrice { get; private set; } = default!;
    public PriceSource PriceSource { get; private set; }

    private ReservationLine(Guid id, Guid reservationId, ReservedItemSnapshot item, Money lineTotalPrice, PriceSource priceSource)
    {
        Id = id;
        ReservationId = reservationId;
        Item = item;
        LineTotalPrice = lineTotalPrice; // need to think about duration - pass from reservation or store duplicate?
        PriceSource = priceSource;
    }

    public static ReservationLine Create(Guid id, Guid reservationId, ReservedItemSnapshot item, Money lineTotalPrice, PriceSource priceSource)
            => new(id, reservationId, item, lineTotalPrice, priceSource);
}


public enum PriceSource
{
    CatalogModel,
    CatalogVariantOverride,
}