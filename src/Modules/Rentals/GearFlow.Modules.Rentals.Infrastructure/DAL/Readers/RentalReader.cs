using GearFlow.Modules.Rentals.Application.Queries;
using GearFlow.Modules.Rentals.Application.Queries.Filters;
using GearFlow.Modules.Rentals.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Rentals.Infrastructure.DAL.Readers;

internal sealed class RentalReader : IRentalReader
{
    private readonly RentalsDbContext _dbContext;

    public RentalReader(RentalsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Rental>> GetAsync(RentalReadFilter filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.Rentals
            .Include(x => x.RentalLines)
            .AsNoTracking();

        if (filter.RentalId.HasValue)
            query = query.Where(x => x.Id == filter.RentalId.Value);

        if (filter.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == filter.CustomerId.Value);

        if (filter.ReservationId.HasValue)
            query = query.Where(x => x.ReservationId == filter.ReservationId.Value);

        if (filter.Statuses is not null && filter.Statuses.Any())
            query = query.Where(x => filter.Statuses.Contains(x.Status));

        if (filter.StartsOnOrAfter.HasValue)
            query = query.Where(x => x.RentalPeriod.Start >= filter.StartsOnOrAfter.Value);

        if (filter.StartsOnOrBefore.HasValue)
            query = query.Where(x => x.RentalPeriod.Start <= filter.StartsOnOrBefore.Value);

        if (filter.EndsOnOrAfter.HasValue)
            query = query.Where(x => x.RentalPeriod.End >= filter.EndsOnOrAfter.Value);

        if (filter.EndsOnOrBefore.HasValue)
            query = query.Where(x => x.RentalPeriod.End <= filter.EndsOnOrBefore.Value);

        if (filter.PickedUpOnOrAfter.HasValue)
            query = query.Where(x => x.PickedUpAt >= filter.PickedUpOnOrAfter.Value);

        if (filter.PickedUpOnOrBefore.HasValue)
            query = query.Where(x => x.PickedUpAt <= filter.PickedUpOnOrBefore.Value);

        if (filter.ReturnedOnOrAfter.HasValue)
            query = query.Where(x => x.ReturnedAt >= filter.ReturnedOnOrAfter.Value);

        if (filter.ReturnedOnOrBefore.HasValue)
            query = query.Where(x => x.ReturnedAt <= filter.ReturnedOnOrBefore.Value);

        query = query
            .OrderBy(x => x.RentalPeriod.Start)
            .ThenBy(x => x.Status);

        return await query.ToListAsync(cancellationToken);
    }
}
