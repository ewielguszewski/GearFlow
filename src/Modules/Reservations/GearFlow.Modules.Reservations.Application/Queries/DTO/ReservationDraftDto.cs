namespace GearFlow.Modules.Reservations.Application.Queries.DTO;

public sealed record ReservationDraftDto(
    Guid DraftId,
    Guid CustomerId,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    DateTime TtlExpiresAt,
    bool IsExpired,
    string Currency,
    decimal TotalPrice,
    IReadOnlyCollection<ReservedItemDto> ReservedItems
    );