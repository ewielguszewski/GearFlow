using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Availability.Contracts;

public interface IAvailabilityReader
{
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableCountsByVariantAsync(
        IReadOnlyCollection<VariantAvailabilityCandidate> candidates,
        DateRange period,
        CancellationToken cancellationToken);
}
