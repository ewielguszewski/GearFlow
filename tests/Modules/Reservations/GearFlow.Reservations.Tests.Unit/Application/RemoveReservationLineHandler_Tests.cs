using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Reservations.Application.Commands.RemoveReservationLine;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Modules.Reservations.Domain.ValueObjects;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;

namespace GearFlow.Reservations.Tests.Unit.Application;

public class RemoveReservationLineHandler_Tests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task remove_line_should_release_held_item_allocation()
    {
        var itemId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var reservation = CreateDraft();
        reservation.AddReservationLine(lineId, CreateOfferSnapshot(itemId), Now);
        var availability = new FakeAvailabilityAllocator();
        var handler = new RemoveReservationLineHandler(new FakeReservationRepository(reservation), availability, new FixedClock(Now));

        await handler.HandleAsync(new RemoveReservationLineCommand(reservation.Id, lineId), CancellationToken.None);

        reservation.ReservationLines.ShouldBeEmpty();
        availability.ReleasedSourceId.ShouldBe(reservation.Id);
        availability.ReleasedItemId.ShouldBe(itemId);
    }

    [Fact]
    public async Task missing_line_should_be_noop()
    {
        var reservation = CreateDraft();
        var availability = new FakeAvailabilityAllocator();
        var handler = new RemoveReservationLineHandler(new FakeReservationRepository(reservation), availability, new FixedClock(Now));

        await handler.HandleAsync(new RemoveReservationLineCommand(reservation.Id, Guid.NewGuid()), CancellationToken.None);

        availability.ReleasedSourceId.ShouldBeNull();
        availability.ReleasedItemId.ShouldBeNull();
    }

    private static Reservation CreateDraft()
        => Reservation.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateRange(Now.Date.AddDays(1), Now.Date.AddDays(2)),
            CurrencyCode.PLN,
            Now);

    private static OfferSnapshot CreateOfferSnapshot(Guid itemId)
        => OfferSnapshot.Create(
            itemId,
            Guid.NewGuid(),
            "Variant",
            "Brand",
            "Model",
            "Public note",
            Money.CreateFromPln(100),
            PriceSource.CatalogModel,
            "M");

    private sealed class FakeReservationRepository : IReservationRepository
    {
        private readonly Reservation _reservation;

        public FakeReservationRepository(Reservation reservation)
        {
            _reservation = reservation;
        }

        public Task<Reservation?> GetAsync(Guid id, CancellationToken ct)
            => Task.FromResult<Reservation?>(_reservation.Id == id ? _reservation : null);

        public void Add(Reservation reservation)
        {
        }

        public void Update(Reservation reservation)
        {
        }

        public Task<Reservation?> GetDraftByCustomerIdAsync(Guid customerId, CancellationToken ct)
            => Task.FromResult<Reservation?>(_reservation.CustomerId == customerId && _reservation.IsDraft ? _reservation : null);
    }

    private sealed class FakeAvailabilityAllocator : IAvailabilityAllocator
    {
        public Guid? ReleasedSourceId { get; private set; }
        public Guid? ReleasedItemId { get; private set; }

        public Task<Guid?> TryAllocateItemAsync(IEnumerable<Guid> itemIds, Guid variantId, Guid sourceId, DateRange timePeriod, CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(null);

        public Task ReleaseReservationAllocationsAsync(Guid sourceId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ReleaseReservationItemAllocationAsync(Guid sourceId, Guid itemId, CancellationToken cancellationToken = default)
        {
            ReleasedSourceId = sourceId;
            ReleasedItemId = itemId;
            return Task.CompletedTask;
        }
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
