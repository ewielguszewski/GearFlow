namespace GearFlow.Modules.Rentals.Application.Queries.DTO;

public sealed record RentalDto(
    Guid RentalId,
    Guid ReservationId,
    Guid CustomerId,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    DateTime PickedUpAt,
    DateTime? ReturnedAt,
    string Currency,
    decimal TotalPrice,
    decimal LateFee,
    decimal DamageFeeTotal,
    IReadOnlyCollection<RentalLineDto> Lines);
