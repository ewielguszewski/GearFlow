
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Domain.Entities;

// Snapshot used for historical display and navigation.
// VariantId point to the public catalog path; ItemId points to the concrete item held internally.
public readonly record struct OfferSnapshot(
    Guid ItemId,
    Guid VariantId,
    string? VariantName,
    string Brand,
    string Model,
    string? PublicNote,
    Money UnitPrice,
    PriceSource PriceSource,

    string? Size
    );

public enum PriceSource
{
    CatalogModel,
    CatalogVariantOverride,
}