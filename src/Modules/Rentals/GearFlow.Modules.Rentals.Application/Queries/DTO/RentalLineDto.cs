namespace GearFlow.Modules.Rentals.Application.Queries.DTO;

public sealed record RentalLineDto(
    Guid RentalLineId,
    Guid ReservationLineId,
    Guid ItemId,
    Guid VariantId,
    string Brand,
    string Model,
    string? Size,
    string? VariantName,
    decimal LineTotalPrice,
    string PickupCondition,
    string? ReturnCondition,
    decimal DamageFee);
