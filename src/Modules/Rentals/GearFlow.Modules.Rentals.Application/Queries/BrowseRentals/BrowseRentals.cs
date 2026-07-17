using GearFlow.Modules.Rentals.Application.Queries.DTO;
using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Shared.Abstractions.Queries;

namespace GearFlow.Modules.Rentals.Application.Queries.BrowseRentals;

public sealed record BrowseRentals(
    Guid? CustomerId,
    Guid? ReservationId,
    RentalStatus? Status,
    DateTime? From,
    DateTime? To,
    RentalLifecycleState? LifecycleState) : IQuery<IEnumerable<RentalDto>>;

public enum RentalLifecycleState
{
    Active,
    Overdue,
    Closed
}
