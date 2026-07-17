using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Modules.Rentals.Domain.ValueObjects;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.Kernel.Types;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Rentals.Domain.Entities;

public class Rental : AggregateRoot
{
    private readonly List<RentalLine> _rentalLines = new();

    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateRange RentalPeriod { get; private set; } = default!;
    public RentalStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime PickedUpAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public CurrencyCode Currency { get; private set; } = default!;
    public Money TotalPrice { get; private set; } = default!;
    public Money LateFee { get; private set; } = default!;

    public int LengthInDays => (RentalPeriod.End - RentalPeriod.Start).Days + 1;
    public IReadOnlyCollection<RentalLine> RentalLines => _rentalLines;

    private Rental() { }

    private Rental(
        Guid id,
        Guid reservationId,
        Guid customerId,
        DateRange rentalPeriod,
        CurrencyCode currency,
        DateTime pickedUpAt)
    {
        Id = id;
        ReservationId = reservationId;
        CustomerId = customerId;
        RentalPeriod = rentalPeriod;
        Currency = currency;
        Status = RentalStatus.Active;
        CreatedAt = pickedUpAt;
        PickedUpAt = pickedUpAt;
        TotalPrice = Money.ZeroFromCurrency(currency);
        LateFee = Money.ZeroFromCurrency(currency);
    }

    public static Rental StartFromReservation(
        Guid id,
        Guid reservationId,
        Guid customerId,
        DateRange rentalPeriod,
        CurrencyCode currency,
        IReadOnlyCollection<RentalLinePickup> lines,
        DateTime pickedUpAt)
    {
        if (lines.Count == 0)
            throw new DomainException("Rental must contain at least one line.");

        var rental = new Rental(id, reservationId, customerId, rentalPeriod, currency, pickedUpAt);

        foreach (var line in lines)
            rental.AddLine(line, pickedUpAt);

        rental.RecalculateTotalPrice();

        return rental;
    }

    public void CompleteReturn(
        IReadOnlyCollection<RentalLineReturn> returnedLines,
        Money lateFee,
        DateTime returnedAt)
    {
        EnsureActive();

        if (lateFee.Currency != Currency)
            throw new DomainException("Late fee currency must match rental currency.");

        if (returnedLines.Count == 0)
            throw new DomainException("Returned rental lines cannot be empty.");

        foreach (var returnedLine in returnedLines)
        {
            var line = _rentalLines.FirstOrDefault(x => x.Id == returnedLine.RentalLineId);

            if (line is null)
                throw new DomainException("Returned rental line does not belong to this rental.");

            line.RecordReturn(
                returnedLine.ReturnCondition,
                returnedLine.ReturnNote,
                returnedLine.DamageFee,
                returnedAt);
        }

        if (_rentalLines.Any(x => !x.IsReturned))
            throw new DomainException("All rental lines must be returned before closing rental.");

        LateFee = lateFee;
        ReturnedAt = returnedAt;
        UpdatedAt = returnedAt;
        Status = RentalStatus.Closed;
    }

    private void AddLine(RentalLinePickup line, DateTime pickedUpAt)
    {
        if (line.LineTotalPrice.Currency != Currency)
            throw new DomainException("Rental line currency must match rental currency.");

        if (line.Item.UnitPrice.Currency != Currency)
            throw new DomainException("Rental item unit price currency must match rental currency.");

        _rentalLines.Add(RentalLine.Create(
            line.RentalLineId,
            Id,
            line.ReservationLineId,
            line.Item,
            line.LineTotalPrice,
            line.PickupCondition,
            line.PickupConditionNote,
            pickedUpAt));
    }

    private void EnsureActive()
    {
        if (Status != RentalStatus.Active)
            throw new DomainException("Rental is not active.");
    }

    private void RecalculateTotalPrice()
        => TotalPrice = _rentalLines
            .Select(x => x.LineTotalPrice)
            .Aggregate(Money.ZeroFromCurrency(Currency), (total, lineTotal) => total.Add(lineTotal));
}