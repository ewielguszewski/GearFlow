using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Availability.Contracts;

public interface IAvailabilityAllocator
{
    Task<Guid?> TryAllocateItemAsync(IEnumerable<Guid> itemIds, Guid SourceId, DateRange timePeriod, CancellationToken cancellationToken = default);
    Task ReleaseReservationAllocationsAsync(Guid SourceId, CancellationToken cancellationToken = default);
}
