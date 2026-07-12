namespace GearFlow.Modules.Reservations.Application.Queries.DTO;

public sealed record UpcomingReservationDto(
    Guid ReservationId,
    Guid CustomerId,
    DateTime StartDate,
    DateTime EndDate,
    string Currency,
    decimal TotalPrice,
    IReadOnlyCollection<ReservedItemDto> ReservedItems
    );