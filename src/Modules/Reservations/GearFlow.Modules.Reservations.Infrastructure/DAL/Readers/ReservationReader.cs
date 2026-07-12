using GearFlow.Modules.Reservations.Application.Queries;
using GearFlow.Modules.Reservations.Application.Queries.Filters;
using GearFlow.Modules.Reservations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Reservations.Infrastructure.DAL.Readers;

public class ReservationReader(ReservationsDbContext dbContext) : IReservationReader
{
    public async Task<IReadOnlyCollection<Reservation>> GetAsync(ReservationReadFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.Reservations
            .Include(r => r.ReservationLines)
            .AsNoTracking();

        if (filter.CustomerId.HasValue)
            query = query.Where(r => r.CustomerId == filter.CustomerId.Value);

        if (filter.Statuses is not null && filter.Statuses.Any())
            query = query.Where(r => filter.Statuses.Contains(r.Status));

        if (filter.CancReason.HasValue)
            query = query.Where(r => r.CancReason == filter.CancReason.Value);

        if (filter.StartsOnOrAfter.HasValue)
            query = query.Where(r => r.ReservedPeriod.Start >= filter.StartsOnOrAfter.Value);
        
        if (filter.StartsOnOrBefore.HasValue)
            query = query.Where(r => r.ReservedPeriod.Start <= filter.StartsOnOrBefore.Value);

        if (filter.EndsOnOrAfter.HasValue)
            query = query.Where(r => r.ReservedPeriod.End >= filter.EndsOnOrAfter.Value);
        
        if (filter.EndsOnOrBefore.HasValue)
            query = query.Where(r => r.ReservedPeriod.End <= filter.EndsOnOrBefore.Value);

        query = query.OrderBy(r => r.ReservedPeriod.Start)
            .ThenBy(r => r.Status);

        return await query.ToListAsync(cancellationToken);
    }
}
