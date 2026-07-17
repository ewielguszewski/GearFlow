using GearFlow.Modules.Rentals.Domain.Entities;
using GearFlow.Modules.Rentals.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Rentals.Infrastructure.DAL.Repositories;

internal sealed class RentalRepository : IRentalRepository
{
    private readonly RentalsDbContext _dbContext;

    public RentalRepository(RentalsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Rental?> GetAsync(Guid id, CancellationToken cancellationToken)
        => _dbContext.Rentals
            .Include(x => x.RentalLines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Rental?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken)
        => _dbContext.Rentals
            .Include(x => x.RentalLines)
            .FirstOrDefaultAsync(x => x.ReservationId == reservationId, cancellationToken);

    public void Add(Rental rental)
        => _dbContext.Rentals.Add(rental);

    public void Update(Rental rental)
        => _dbContext.Rentals.Update(rental);
}
