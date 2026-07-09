using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Reservations.Infrastructure.DAL.Repositories;

internal sealed class ReservationRepository : IReservationRepository
{
    private readonly ReservationsDbContext _dbContext;

    public ReservationRepository(ReservationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Reservation?> GetAsync(Guid id, CancellationToken ct)
        => _dbContext.Reservations
            .Include(x => x.ReservationLines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public void Add(Reservation reservation)
        => _dbContext.Reservations.Add(reservation);

    public void Update(Reservation reservation)
        => _dbContext.Reservations.Update(reservation);

    public Task<Reservation?> GetDraftByCustomerIdAsync(Guid customerId, CancellationToken ct)
        => _dbContext.Reservations
            .Include(x => x.ReservationLines)
            .Where(x => x.CustomerId == customerId && x.Status == ReservationStatus.Draft)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyCollection<Reservation>> GetExpiredDraftsAsync(DateTime utcNow, int batchSize, CancellationToken ct)
        => await _dbContext.Reservations
            .Include(x => x.ReservationLines)
            .Where(x => x.Status == ReservationStatus.Draft && x.TtlExpiresAt < utcNow)
            .OrderBy(x => x.TtlExpiresAt)
            .Take(batchSize)
            .ToArrayAsync(ct);
}