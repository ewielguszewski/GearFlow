using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Availability.Contracts;

public interface IAvailabilityAllocator
{
    Task<Guid?> TryAllocateItemAsync(IEnumerable<Guid> itemIds, Guid variantId, Guid sourceId, DateRange timePeriod, CancellationToken cancellationToken = default);
    Task ReleaseReservationAllocationsAsync(Guid sourceId, CancellationToken cancellationToken = default);
}