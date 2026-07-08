using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;

namespace GearFlow.Reservations.Tests.Unit.Application;

public class AddReservationLineHandler_Tests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task expired_draft_should_fail_without_cleanup_side_effects()
    {
        var reservation = CreateDraft();
        var repository = new FakeReservationRepository(reservation);
        var availability = new FakeAvailabilityAllocator();
        var handler = CreateHandler(repository, new FakeCatalogOfferReader(), availability, reservation.TtlExpiresAt.AddMinutes(1));

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(new AddReservationLineCommand(
            reservation.Id,
            Guid.NewGuid(),
            Guid.NewGuid()
            )));

        exception.ShouldBeOfType<DomainException>();
        availability.ReleasedSourceId.ShouldBeNull();
        reservation.Status.ShouldBe(ReservationStatus.Draft);
        reservation.CancReason.ShouldBeNull();
    }

    [Fact]
    public async Task add_line_should_pass_variant_id_to_availability_allocator()
    {
        var reservation = CreateDraft();
        var variantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var repository = new FakeReservationRepository(reservation);
        var catalog = new FakeCatalogOfferReader
        {
            Offer = new ReservableOfferDto(
                variantId,
                "Premium",
                "Brand",
                "Model",
                "Public note",
                Money.CreateFromPln(100),
                null,
                "M",
                [itemId])
        };
        var availability = new FakeAvailabilityAllocator
        {
            ItemToAllocate = itemId
        };
        var handler = CreateHandler(repository, catalog, availability, reservation.CreatedAt);
        var reservationLineId = Guid.NewGuid();

        await handler.HandleAsync(new AddReservationLineCommand(
            reservation.Id,
            reservationLineId,
            variantId));

        availability.AllocatedVariantId.ShouldBe(variantId);
        availability.AllocatedSourceId.ShouldBe(reservation.Id);
        reservation.ReservationLines.Single().Id.ShouldBe(reservationLineId);
    }

    private static AddReservationLineHandler CreateHandler(
        IReservationRepository repository,
        ICatalogOfferReader catalogOfferReader,
        IAvailabilityAllocator availabilityAllocator,
        DateTime now)
        => new(repository, catalogOfferReader, availabilityAllocator, new FixedClock(now));

    private static Reservation CreateDraft()
        => Reservation.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateRange(Now.Date.AddDays(1), Now.Date.AddDays(3)),
            CurrencyCode.PLN,
            Now);

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

    private sealed class FakeCatalogOfferReader : ICatalogOfferReader
    {
        public ReservableOfferDto? Offer { get; init; }

        public Task<ReservableOfferDto?> GetReservableOfferAsync(Guid offerVariantId, CancellationToken cancellationToken = default)
            => Task.FromResult(Offer);

        public Task<IReadOnlyCollection<CatalogOfferCandidateDto>> SearchOfferCandidatesAsync(OfferSearchCriteria criteria, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CatalogOfferCandidateDto>>([]);
    }

    private sealed class FakeAvailabilityAllocator : IAvailabilityAllocator
    {
        public Guid? AllocatedVariantId { get; private set; }
        public Guid? AllocatedSourceId { get; private set; }
        public Guid? ReleasedSourceId { get; private set; }
        public Guid? ItemToAllocate { get; init; }

        public Task<Guid?> TryAllocateItemAsync(IEnumerable<Guid> itemIds, Guid variantId, Guid sourceId, DateRange timePeriod, CancellationToken cancellationToken = default)
        {
            AllocatedVariantId = variantId;
            AllocatedSourceId = sourceId;

            return Task.FromResult(ItemToAllocate);
        }

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
