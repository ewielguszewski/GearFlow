using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Shared.Abstractions.Queries;

namespace GearFlow.Modules.Reservations.Application.Queries.GetUpcomingReservationsForUser;

public record GetUpcomingReservations(Guid? TargetCustomerId) : IQuery<IEnumerable<UpcomingReservationDto>>;