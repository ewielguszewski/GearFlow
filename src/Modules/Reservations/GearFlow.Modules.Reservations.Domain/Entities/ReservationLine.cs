using GearFlow.Modules.Reservations.Domain.ValueObjects;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Domain.Entities;


// Snapshot of the selected catalog offer and concrete item.
public class ReservationLine
{
    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Reservation Reservation { get; private set; } = default!;

    // Stores the selected offer variant, and held item without referencing live Catalog entities.
    public OfferSnapshot Item { get; private set; } = default!;
    

    public Money LineTotalPrice { get; private set; } = default!;

    private ReservationLine() { } // EF

    private ReservationLine(Guid id, Guid reservationId, OfferSnapshot item, Money lineTotalPrice)
    {
        Id = id;
        ReservationId = reservationId;
        Item = item;
        LineTotalPrice = lineTotalPrice; // need to think about duration - pass from reservation or store duplicate?
    }

    public static ReservationLine Create(Guid id, Guid reservationId, OfferSnapshot item, Money lineTotalPrice)
            => new(id, reservationId, item, lineTotalPrice);
}
