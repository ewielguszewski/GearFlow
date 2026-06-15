namespace GearFlow.Modules.Availability.Contracts;

public sealed record VariantAvailabilityCandidate
(
    Guid VariantId,
    IReadOnlyCollection<Guid> ActiveItemIds
);
