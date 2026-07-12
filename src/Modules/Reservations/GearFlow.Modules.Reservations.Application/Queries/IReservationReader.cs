using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Application.Queries.Filters;
using GearFlow.Modules.Reservations.Domain.Entities;

namespace GearFlow.Modules.Reservations.Application.Queries;

public interface IReservationReader
{
    Task<IReadOnlyCollection<Reservation>> GetAsync(ReservationReadFilter filter, CancellationToken cancellationToken);
}
