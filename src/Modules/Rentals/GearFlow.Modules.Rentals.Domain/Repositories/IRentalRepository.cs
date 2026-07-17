using GearFlow.Modules.Rentals.Domain.Entities;

namespace GearFlow.Modules.Rentals.Domain.Repositories;

public interface IRentalRepository
{
    Task<Rental?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Rental?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken);
    void Add(Rental rental);
    void Update(Rental rental);
}
