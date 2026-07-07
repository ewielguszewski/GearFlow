namespace GearFlow.Modules.Reservations.Application.Queries.GetReservationDraft;

public sealed record ReservedItemDto(
    Guid ReservationLineId,
    Guid VariantId,
    string Model,
    string Brand,
    decimal BasePrice,
    decimal LineTotalPrice
    );