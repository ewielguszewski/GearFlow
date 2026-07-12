using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Application.Queries.GetAvailableOffers;

public sealed record AvailableOfferResult(
    Guid VariantId,
    string Brand,
    string Model,
    string Type,
    Money PricePerDay,
    string? Size,
    int AvailableCount
    );