using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Availability.Contracts;

public interface IAvailabilityAllocator
{
    Task<Guid?> AllocateItemAsync(Guid offerVariantId, DateRange timePeriod, Guid reservationLineId, CancellationToken cancellationToken = default);
}
