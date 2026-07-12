namespace GearFlow.Modules.Reservations.Application.Queries.DTO;

public sealed record ReservedItemDto(
    Guid ReservationLineId,
    Guid VariantId,
    string Model,
    string Brand,
    decimal BasePrice,
    decimal LineTotalPrice
    );