using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Contracts;

public sealed record CatalogOfferCandidateDto
(
    Guid VariantId,
    string Brand,
    string Model,
    string Type,
    Money PricePerDay,
    string? Size,
    IReadOnlyCollection<Guid> ActiveItemIds
);