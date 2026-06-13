using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Domain.Entities;


public class Reservation
{
    public static readonly int MaxTtl = 15;                // todo: move TTL values to reservation options when configuration is introduced
    public static readonly int TtlBufferMinutes = 5;
    public static readonly int TtlUpdateBufferMinutes = 3;

    private readonly List<ReservationLine> _reservationLines = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateRange ReservedPeriod { get; private set; } = default!;

    public ReservationStatus Status { get; private set; }
    public PaymentMethod? SelectedPaymentMethod { get; private set; } // simplified for now until implementing payments module

    public DateTime CreatedAt { get; }
    public DateTime TtlExpiresAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }


    // reservation totals use one currency; cross-currency conversion is out of scope by now.
    public CurrencyCode Currency { get; private set; }
    public Money PaidAmount { get; private set;}
    public Money TotalPrice { get; private set; }

    // tracking deposit amount needed to know if reservation can be confirmed if payment method is credit card; cash on pickup doesn't require deposit
    // for now may assume that all individual priced items require deposit

    public int LengthInDays => (ReservedPeriod.End - ReservedPeriod.Start).Days + 1; // inclusive end date, so add 1 day
    public Money RemainingToPay => TotalPrice.Subtract(PaidAmount);

    public IReadOnlyCollection<ReservationLine> ReservationLines => _reservationLines;

    private Reservation(Guid id, Guid customerId, DateRange reservedPeriod, CurrencyCode currency)
    {
        Id = id;
        CustomerId = customerId;
        ReservedPeriod = reservedPeriod;
        Status = ReservationStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        TtlExpiresAt = CreatedAt.AddMinutes(TtlBufferMinutes);
        Currency = currency;
        TotalPrice = Money.ZeroFromCurrency(Currency);
        PaidAmount = Money.ZeroFromCurrency(Currency);
    }

    public static Reservation CreateDraft(Guid id, Guid customerId, DateRange reservedPeriod, CurrencyCode currency)
        => new Reservation(id, customerId, reservedPeriod, currency);

    public void RemoveReservationLine(Guid reservationLineId, DateTime utcNow)
    {
        EnsureDraftNotExpired(utcNow);
        
        var reservationLine = _reservationLines.FirstOrDefault(rl => rl.Id == reservationLineId);
        if (reservationLine == null)
            return;

        _reservationLines.Remove(reservationLine!);
        RecalculateTotalPrice();
        ExtendTtlIfNotExceeded();
    }

    public void AddReservationLine(Guid reservationLineId, OfferSnapshot item, DateTime utcNow)
    {
        if (item.UnitPrice.Currency != Currency)
            throw new DomainException($"Currency of the reservation line ({item.UnitPrice.Currency}) must match reservation currency ({Currency})");

        EnsureDraftNotExpired(utcNow);

        var lineTotalPrice = Money.Create(item.UnitPrice.Amount * LengthInDays, item.UnitPrice.Currency);

        var reservationLine = ReservationLine.Create(reservationLineId, Id, item, lineTotalPrice);

        _reservationLines.Add(reservationLine);

        RecalculateTotalPrice();
        ExtendTtlIfNotExceeded();
    }

    public void CancelReservation()
    {
        if (Status != ReservationStatus.Fulfilled)
        {
            Status = ReservationStatus.Cancelled;
        }
    }

    public void MarkAsPendingPayment(PaymentMethod paymentMethod, DateTime utcNow)
    {
        EnsureDraftNotExpired(utcNow);

        SelectedPaymentMethod = paymentMethod;
        Status = ReservationStatus.PendingPayment;
    }

    public void MarkAsConfirmed()
    {
        if (SelectedPaymentMethod == null)
            throw new DomainException("Payment method was not selected");

        if (SelectedPaymentMethod == PaymentMethod.CashOnPickup)
            Status = ReservationStatus.Confirmed;

        // todo: handle deposit here when implementing deposit policy, for now just mark as confirmed as well
        
        Status = ReservationStatus.Confirmed;

    }

    public void MarkAsFulfilled()
    {
        if (Status == ReservationStatus.Confirmed)
        {
            Status = ReservationStatus.Fulfilled;
        }
    }

    public void SetUpdatedAt(DateTime utcNow)
        => UpdatedAt = utcNow;

    private void EnsureDraftNotExpired(DateTime utcNow)
    {
        if (Status != ReservationStatus.Draft || utcNow > TtlExpiresAt)
            throw new DomainException("Reservation has expired.");
    }

    private void RecalculateTotalPrice()
        => TotalPrice = ReservationLines
                .Select(rl => rl.LineTotalPrice)
                .Aggregate(Money.ZeroFromCurrency(Currency), (total, lineTotal) => total.Add(lineTotal));

    private void ExtendTtlIfNotExceeded()
    {
        var maxExpiration = CreatedAt.AddMinutes(MaxTtl);
        var proposedExpiration = TtlExpiresAt.AddMinutes(TtlUpdateBufferMinutes);

        if (maxExpiration > proposedExpiration)
            TtlExpiresAt = proposedExpiration;
    }
}


public enum ReservationStatus
{
    Draft,
    PendingPayment,
    Confirmed,
    Fulfilled, // Fulfilled means the reservation has been consumed by the pickup/rental workflow.
    Cancelled, 
}

public enum PaymentMethod
{
    CreditCard,
    CashOnPickup
}