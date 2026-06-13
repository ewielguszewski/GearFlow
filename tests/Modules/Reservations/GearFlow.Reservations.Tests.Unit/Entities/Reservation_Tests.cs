using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;

namespace GearFlow.Reservations.Tests.Unit.Entities;

public class Reservation_Tests
{
    [Fact]
    public void should_not_add_line_after_ttl()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);

        var exception = Record.Exception(() => 
            reservation.AddReservationLine(Guid.NewGuid(), _offerSnapshot, DateTime.UtcNow.AddMinutes(Reservation.TtlBufferMinutes)));

        exception.ShouldNotBeNull();
        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void should_not_remove_line_after_ttl()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);
        reservation.AddReservationLine(_reservationLine.Id, _offerSnapshot, DateTime.UtcNow);

        var exception = Record.Exception(() =>
            reservation.RemoveReservationLine(_reservationLine.Id, reservation.TtlExpiresAt.AddMinutes(1)));

        exception.ShouldNotBeNull();
        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void adding_and_removing_reservation_line_should_recalculate_total()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);

        reservation.AddReservationLine(_reservationLine.Id, _offerSnapshot, DateTime.UtcNow);

        Assert.True(reservation.TotalPrice.Amount == _price.Amount);

        reservation.RemoveReservationLine(_reservationLine.Id, DateTime.UtcNow);

        Assert.True(reservation.TotalPrice.Amount == 0);
    }

    [Fact]
    public void adding_and_removing_reservation_line_should_extend_ttl()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);

        Assert.True(reservation.TtlExpiresAt == reservation.CreatedAt.AddMinutes(Reservation.TtlBufferMinutes));

        reservation.AddReservationLine(Guid.NewGuid(), _offerSnapshot, DateTime.UtcNow);

        Assert.True(reservation.TtlExpiresAt == reservation.CreatedAt.AddMinutes(Reservation.TtlBufferMinutes + Reservation.TtlUpdateBufferMinutes));

    }

    [Fact]
    public void exceeding_max_ttl_should_not_increase_ttl()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);

        int noOfTimes = (Reservation.MaxTtl - Reservation.TtlBufferMinutes) / Reservation.TtlUpdateBufferMinutes;

        for (int i = 0; i < noOfTimes + 1; i++)
        {
            reservation.AddReservationLine(Guid.NewGuid(), _offerSnapshot, DateTime.UtcNow);
        }

        Assert.True(reservation.TtlExpiresAt <= reservation.CreatedAt.AddMinutes(Reservation.MaxTtl));
    }

    [Fact]
    public void pending_payment_should_select_payment_method_and_change_status()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);

        reservation.MarkAsPendingPayment(PaymentMethod.CreditCard, DateTime.UtcNow);

        reservation.Status.ShouldBe(ReservationStatus.PendingPayment);
        reservation.SelectedPaymentMethod.ShouldBe(PaymentMethod.CreditCard);
    }

    [Fact]
    public void draft_without_payment_method_should_not_be_confirmed()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);

        var exception = Record.Exception(() => reservation.MarkAsConfirmed());

        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void pending_payment_reservation_should_be_confirmed()
    {
        var reservation = Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), _reservedPeriod, _currency);
        reservation.MarkAsPendingPayment(PaymentMethod.CreditCard, DateTime.UtcNow);

        reservation.MarkAsConfirmed();

        reservation.Status.ShouldBe(ReservationStatus.Confirmed);
    }

    private readonly PriceSource _priceSource;
    private readonly CurrencyCode _currency;
    private readonly Money _price;
    private readonly DateRange _reservedPeriod;
    private readonly OfferSnapshot _offerSnapshot;
    private readonly ReservationLine _reservationLine;

    public Reservation_Tests()
    {
        _priceSource = PriceSource.CatalogModel;
        _currency = CurrencyCode.PLN;
        _price = Money.CreateFromPln(1);
        _reservedPeriod = new DateRange(DateTime.UtcNow, DateTime.UtcNow);
        _offerSnapshot = new OfferSnapshot(Guid.NewGuid(), Guid.NewGuid(), "Name", "Brand", "Model", "Note", _price, _priceSource, "S");
        _reservationLine = ReservationLine.Create(Guid.NewGuid(), Guid.NewGuid(), _offerSnapshot, _price);
    }
}
