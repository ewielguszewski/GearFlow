using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Queries;

namespace GearFlow.Modules.Reservations.Application.Queries.BrowseAdminReservations;

public sealed record BrowseAdminReservations(
    Guid? CustomerId,
    ReservationStatus? Status,
    CancellationReason? CancellationReason,
    DateTime? From,
    DateTime? To,
    ReservationPickupState? PickupState
    ) : IQuery<IEnumerable<AdminReservationDto>>;

public enum ReservationPickupState
{
    Upcoming,
    Overdue
}
