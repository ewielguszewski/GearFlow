
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Domain.Entities;

// Snapshot used for historical display and navigation.
// OfferVariantId point to the public catalog path; HeldItemId points to the concrete item held internally.
public readonly record struct ReservedItemSnapshot(
    Guid ItemId,
    Guid OfferVariantId,
    string? ItemName,
    string Brand,
    string Model,
    string? PublicNote,
    Money UnitPrice,

    string? Size
    );

