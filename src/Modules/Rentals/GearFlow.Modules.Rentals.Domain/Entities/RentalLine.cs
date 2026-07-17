using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Rentals.Domain.Entities;

public class RentalLine
{
    public Guid Id { get; private set; }
    public Guid RentalId { get; private set; }
    public Rental Rental { get; private set; } = default!;

    public Guid ReservationLineId { get; private set; }
    public ItemSnapshot Item { get; private set; } = default!;
    public Money LineTotalPrice { get; private set; } = default!;

    public ItemCondition PickupCondition { get; private set; }
    public string? PickupConditionNote { get; private set; }
    public DateTime PickupConditionRecordedAt { get; private set; }

    public ItemCondition? ReturnCondition { get; private set; }
    public string? ReturnConditionNote { get; private set; }
    public DateTime? ReturnConditionRecordedAt { get; private set; }
    public Money DamageFee { get; private set; } = default!;

    public bool IsReturned => ReturnCondition.HasValue;

    private RentalLine()
    {
    }

    private RentalLine(
        Guid id,
        Guid rentalId,
        Guid reservationLineId,
        ItemSnapshot item,
        Money lineTotalPrice,
        ItemCondition pickupCondition,
        string? pickupConditionNote,
        DateTime pickupConditionRecordedAt)
    {
        if (item.UnitPrice.Currency != lineTotalPrice.Currency)
            throw new DomainException("Rental line item price currency must match line total currency.");

        Id = id;
        RentalId = rentalId;
        ReservationLineId = reservationLineId;
        Item = item;
        LineTotalPrice = lineTotalPrice;
        PickupCondition = pickupCondition;
        PickupConditionNote = pickupConditionNote;
        PickupConditionRecordedAt = pickupConditionRecordedAt;
        DamageFee = Money.ZeroFromCurrency(lineTotalPrice.Currency);
    }

    public static RentalLine Create(
        Guid id,
        Guid rentalId,
        Guid reservationLineId,
        ItemSnapshot item,
        Money lineTotalPrice,
        ItemCondition pickupCondition,
        string? pickupConditionNote,
        DateTime pickupConditionRecordedAt)
        => new(
            id,
            rentalId,
            reservationLineId,
            item,
            lineTotalPrice,
            pickupCondition,
            pickupConditionNote,
            pickupConditionRecordedAt);

    public void RecordReturn(
        ItemCondition returnCondition,
        string? returnConditionNote,
        Money damageFee,
        DateTime recordedAt)
    {
        if (IsReturned)
            throw new DomainException("Rental line has already been returned.");

        if (damageFee.Currency != LineTotalPrice.Currency)
            throw new DomainException("Damage fee currency must match rental line currency.");

        ReturnCondition = returnCondition;
        ReturnConditionNote = returnConditionNote;
        ReturnConditionRecordedAt = recordedAt;
        DamageFee = damageFee;
    }
}
