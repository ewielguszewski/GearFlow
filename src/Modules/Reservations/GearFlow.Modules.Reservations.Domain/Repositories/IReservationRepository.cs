using GearFlow.Modules.Reservations.Domain.Entities;

namespace GearFlow.Modules.Reservations.Domain.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetAsync(Guid id, CancellationToken ct);
    void Add(Reservation reservation);
    void Update(Reservation reservation);

    Task<Reservation?> GetDraftByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task<IReadOnlyCollection<Reservation>> GetExpiredDraftsAsync(DateTime utcNow, int batchSize, CancellationToken ct);
}
