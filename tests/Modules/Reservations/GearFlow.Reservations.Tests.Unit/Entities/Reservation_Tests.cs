using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;

namespace GearFlow.Reservations.Tests.Unit.Entities;

public class Reservation_Tests
{
    private readonly IClock _clock;
    private readonly CurrencyCode _currency;
    private readonly Money _price;
    private readonly DateRange _reservedPeriod;
    private readonly ItemSnapshot _offerSnapshot;

    public Reservation_Tests()
    {
        _clock = new FixedClock(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        _currency = CurrencyCode.PLN;
        _price = Money.CreateFromPln(1);
        _reservedPeriod = new DateRange(_clock.Current().Date, _clock.Current().Date);
        _offerSnapshot = ItemSnapshot.Create(Guid.NewGuid(), Guid.NewGuid(), "Name", "Brand", "Model", "Note", _price, PriceSource.CatalogModel, "S");
    }

    [Fact]
    public void draft_start_date_should_not_be_before_created_date()
    {
        var pastPeriod = new DateRange(_clock.Current().Date.AddDays(-1), _clock.Current().Date);

        var exception = Record.Exception(() => CreateDraft(pastPeriod));

        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void draft_start_date_can_be_same_as_created_date()
    {
        var reservation = CreateDraft();

        reservation.CreatedAt.ShouldBe(_clock.Current());
        reservation.ReservedPeriod.Start.ShouldBe(_clock.Current().Date);
    }

    [Fact]
    public void should_not_add_line_after_ttl()
    {
        var reservation = CreateDraft();

        var exception = Record.Exception(() =>
            reservation.AddReservationLine(Guid.NewGuid(), _offerSnapshot, reservation.TtlExpiresAt.AddMinutes(1)));

        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void should_not_remove_line_after_ttl()
    {
        var reservation = CreateDraft();
        var reservationLineId = AddLine(reservation);

        var exception = Record.Exception(() =>
            reservation.RemoveReservationLine(reservationLineId, reservation.TtlExpiresAt.AddMinutes(1)));

        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void adding_and_removing_reservation_line_should_recalculate_total()
    {
        var reservation = CreateDraft();

        var reservationLineId = AddLine(reservation);

        reservation.TotalPrice.Amount.ShouldBe(_price.Amount);

        reservation.RemoveReservationLine(reservationLineId, _clock.Current());

        reservation.TotalPrice.Amount.ShouldBe(0);
    }

    [Fact]
    public void adding_and_removing_reservation_line_should_extend_ttl()
    {
        var reservation = CreateDraft();

        reservation.TtlExpiresAt.ShouldBe(reservation.CreatedAt.AddMinutes(Reservation.TtlBufferMinutes));

        AddLine(reservation);

        reservation.TtlExpiresAt.ShouldBe(reservation.CreatedAt.AddMinutes(Reservation.TtlBufferMinutes + Reservation.TtlUpdateBufferMinutes));
    }

    [Fact]
    public void exceeding_max_ttl_should_not_increase_ttl()
    {
        var reservation = CreateDraft();

        var noOfTimes = (Reservation.MaxTtl - Reservation.TtlBufferMinutes) / Reservation.TtlUpdateBufferMinutes;

        for (var i = 0; i < noOfTimes + 1; i++)
        {
            AddLine(reservation);
        }

        reservation.TtlExpiresAt.ShouldBeLessThanOrEqualTo(reservation.CreatedAt.AddMinutes(Reservation.MaxTtl));
    }

    [Fact]
    public void pending_payment_should_select_payment_method_and_change_status()
    {
        var reservation = CreateDraft();
        AddLine(reservation);

        reservation.MarkAsPendingPayment(PaymentMethod.CreditCard, _clock.Current());

        reservation.Status.ShouldBe(ReservationStatus.PendingPayment);
        reservation.SelectedPaymentMethod.ShouldBe(PaymentMethod.CreditCard);
    }

    [Fact]
    public void empty_draft_should_not_be_marked_as_pending_payment()
    {
        var reservation = CreateDraft();

        var exception = Record.Exception(() => reservation.MarkAsPendingPayment(PaymentMethod.CreditCard, _clock.Current()));

        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void draft_without_payment_method_should_not_be_confirmed()
    {
        var reservation = CreateDraft();

        var exception = Record.Exception(() => reservation.MarkAsConfirmed());

        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void pending_payment_reservation_should_be_confirmed()
    {
        var reservation = CreateDraft();
        AddLine(reservation);
        reservation.MarkAsPendingPayment(PaymentMethod.CreditCard, _clock.Current());

        reservation.MarkAsConfirmed();

        reservation.Status.ShouldBe(ReservationStatus.Confirmed);
    }

    [Fact]
    public void confirmed_reservation_should_be_cancelled()
    {
        var reservation = CreateDraft();
        AddLine(reservation);
        reservation.MarkAsPendingPayment(PaymentMethod.CreditCard, _clock.Current());

        reservation.CancelReservation(CancellationReason.EmployeeCancelled);

        reservation.Status.ShouldBe(ReservationStatus.Cancelled);
    }

    private Reservation CreateDraft()
        => CreateDraft(_reservedPeriod);

    private Reservation CreateDraft(DateRange reservedPeriod)
        => Reservation.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), reservedPeriod, _currency, _clock.Current());

    private Guid AddLine(Reservation reservation)
    {
        var reservationLineId = Guid.NewGuid();
        reservation.AddReservationLine(reservationLineId, _offerSnapshot, _clock.Current());
        return reservationLineId;
    }

    private sealed class FixedClock : IClock
    {
        private readonly DateTime _now;

        public FixedClock(DateTime now)
        {
            _now = now;
        }

        public DateTime Current() => _now;
    }
}
