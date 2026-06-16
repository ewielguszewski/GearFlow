using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Contracts;

public sealed record ReservableOfferDto
(
    Guid VariantId,
    string? VariantName,
    string Brand,
    string Model,
    string? PublicNote,
    Money BasePrice,
    Money? OverridenPrice,
    string? Size,

    IReadOnlyCollection<Guid> ActiveItemIds
);