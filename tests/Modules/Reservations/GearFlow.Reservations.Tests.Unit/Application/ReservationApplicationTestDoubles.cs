using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Reservations.Tests.Unit.Application;

internal static class ReservationApplicationTestData
{
    public static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public static Reservation CreateDraft(Guid? customerId = null, DateRange? reservedPeriod = null)
        => Reservation.CreateDraft(
            Guid.NewGuid(),
            customerId ?? Guid.NewGuid(),
            reservedPeriod ?? new DateRange(Now.Date.AddDays(1), Now.Date.AddDays(2)),
            CurrencyCode.PLN,
            Now);

    public static ItemSnapshot CreateOfferSnapshot(Guid? itemId = null, Guid? variantId = null)
        => ItemSnapshot.Create(
            itemId ?? Guid.NewGuid(),
            variantId ?? Guid.NewGuid(),
            "Variant",
            "Brand",
            "Model",
            "Public note",
            Money.CreateFromPln(100),
            PriceSource.CatalogModel,
            "M");
}

internal sealed class FixedClock : IClock
{
    private readonly DateTime _now;

    public FixedClock(DateTime now)
    {
        _now = now;
    }

    public DateTime Current() => _now;
}

internal sealed class FakeReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations;

    public FakeReservationRepository(params Reservation[] reservations)
    {
        _reservations = reservations.ToList();
    }

    public Reservation? AddedReservation { get; private set; }

    public Task<Reservation?> GetAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_reservations.SingleOrDefault(x => x.Id == id));

    public void Add(Reservation reservation)
    {
        AddedReservation = reservation;
        _reservations.Add(reservation);
    }

    public void Update(Reservation reservation)
    {
    }

    public Task<Reservation?> GetDraftByCustomerIdAsync(Guid customerId, CancellationToken ct)
        => Task.FromResult(_reservations.SingleOrDefault(x => x.CustomerId == customerId && x.IsDraft));

    public Task<IReadOnlyCollection<Reservation>> GetExpiredDraftsAsync(DateTime utcNow, int batchSize, CancellationToken ct)
    {
        var expiredDrafts = _reservations
            .Where(x => x.IsDraft && x.TtlExpiresAt < utcNow)
            .OrderBy(x => x.TtlExpiresAt)
            .Take(batchSize)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<Reservation>>(expiredDrafts);
    }
}

internal sealed class FakeAvailabilityAllocator : IAvailabilityAllocator
{
    public Guid? AllocatedVariantId { get; private set; }
    public Guid? AllocatedSourceId { get; private set; }
    public Guid? ReleasedSourceId { get; private set; }
    public Guid? ReleasedItemId { get; private set; }
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
    {
        ReleasedSourceId = sourceId;
        ReleasedItemId = itemId;
        return Task.CompletedTask;
    }
}

internal sealed class FakeCatalogOfferReader : ICatalogOfferReader
{
    public ReservableOfferDto? Offer { get; init; }
    public IReadOnlyCollection<CatalogOfferCandidateDto> Candidates { get; init; } = [];

    public Task<ReservableOfferDto?> GetReservableOfferAsync(Guid offerVariantId, CancellationToken cancellationToken = default)
        => Task.FromResult(Offer);

    public Task<IReadOnlyCollection<CatalogOfferCandidateDto>> SearchOfferCandidatesAsync(OfferSearchCriteria criteria, CancellationToken cancellationToken = default)
        => Task.FromResult(Candidates);
}

internal sealed class FakeReservationAuthorizationService : IReservationAuthorizationService
{
    public Reservation? AuthorizedReservation { get; private set; }
    public Guid? ResolvedCustomerId { get; init; }
    public Exception? ExceptionToThrow { get; init; }

    public void Authorize(Reservation reservation)
    {
        AuthorizedReservation = reservation;

        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;
    }

    public Guid ResolveCustomerId(Guid? requestedCustomerId)
        => ResolvedCustomerId ?? requestedCustomerId ?? throw new InvalidOperationException("Customer id was not provided.");
}
