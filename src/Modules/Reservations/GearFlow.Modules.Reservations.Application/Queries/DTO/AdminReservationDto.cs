namespace GearFlow.Modules.Reservations.Application.Queries.DTO;

public sealed record AdminReservationDto(
    Guid ReservationId,
    Guid CustomerId,
    string Status,
    string? CancellationReason,
    DateTime StartDate,
    DateTime EndDate,
    string Currency,
    decimal TotalPrice,
    IReadOnlyCollection<ReservedItemDto> ReservedItems
    );
