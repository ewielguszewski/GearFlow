using GearFlow.Modules.Reservations.Domain.Entities;

namespace GearFlow.Modules.Reservations.Domain.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(Reservation reservation, CancellationToken ct);
    Task UpdateAsync(Reservation reservation, CancellationToken ct);

    Task<Reservation?> GetDraftByCustomerIdAsync(Guid customerId, CancellationToken ct);
}
