using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Modules.Reservations.Domain.ValueObjects;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;

namespace GearFlow.Reservations.Tests.Unit.Application;

public class CreateDraftReservationHandler_Tests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task existing_draft_with_lines_should_be_replaced_and_release_allocations()
    {
        var customerId = Guid.NewGuid();
        var existingDraft = CreateDraft(customerId);
        existingDraft.AddReservationLine(Guid.NewGuid(), CreateOfferSnapshot(), Now);
        var repository = new FakeReservationRepository(existingDraft);
        var availability = new FakeAvailabilityAllocator();
        var handler = new CreateDraftReservationHandler(repository, availability, new FixedClock(Now));
        var newReservationId = Guid.NewGuid();

        await handler.HandleAsync(new CreateDraftReservationCommand(
            newReservationId,
            customerId,
            Now.Date.AddDays(3),
            Now.Date.AddDays(5),
            "PLN"));

        availability.ReleasedSourceId.ShouldBe(existingDraft.Id);
        existingDraft.Status.ShouldBe(ReservationStatus.Cancelled);
        existingDraft.CancReason.ShouldBe(CancellationReason.ReplacedByNewDraft);
        repository.AddedReservation.ShouldNotBeNull();
        repository.AddedReservation.Id.ShouldBe(newReservationId);
    }

    private static Reservation CreateDraft(Guid customerId)
        => Reservation.CreateDraft(
            Guid.NewGuid(),
            customerId,
            new DateRange(Now.Date.AddDays(1), Now.Date.AddDays(2)),
            CurrencyCode.PLN,
            Now);

    private static OfferSnapshot CreateOfferSnapshot()
        => OfferSnapshot.Create(
            Guid.NewGuid(),
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
        private readonly Reservation? _existingDraft;

        public FakeReservationRepository(Reservation? existingDraft)
        {
            _existingDraft = existingDraft;
        }

        public Reservation? AddedReservation { get; private set; }

        public Task<Reservation?> GetAsync(Guid id, CancellationToken ct)
            => Task.FromResult<Reservation?>(_existingDraft?.Id == id ? _existingDraft : null);

        public void Add(Reservation reservation)
            => AddedReservation = reservation;

        public void Update(Reservation reservation)
        {
        }

        public Task<Reservation?> GetDraftByCustomerIdAsync(Guid customerId, CancellationToken ct)
            => Task.FromResult(_existingDraft is not null
                               && _existingDraft.CustomerId == customerId
                               && _existingDraft.IsDraft
                ? _existingDraft
                : null);
    }

    private sealed class FakeAvailabilityAllocator : IAvailabilityAllocator
    {
        public Guid? ReleasedSourceId { get; private set; }

        public Task<Guid?> TryAllocateItemAsync(IEnumerable<Guid> itemIds, Guid variantId, Guid sourceId, DateRange timePeriod, CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(null);

        public Task ReleaseReservationAllocationsAsync(Guid sourceId, CancellationToken cancellationToken = default)
        {
            ReleasedSourceId = sourceId;
            return Task.CompletedTask;
        }

        public Task ReleaseReservationItemAllocationAsync(Guid sourceId, Guid itemId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
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
