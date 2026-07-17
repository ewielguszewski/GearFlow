using GearFlow.Modules.Rentals.Application.Queries.DTO;
using GearFlow.Modules.Rentals.Application.Queries.Filters;
using GearFlow.Modules.Rentals.Application.Queries.Mappers;
using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Rentals.Application.Queries.BrowseRentals;

public sealed class BrowseRentalsHandler : IQueryHandler<BrowseRentals, IEnumerable<RentalDto>>
{
    private readonly IRentalReader _rentalReader;
    private readonly IClock _clock;

    public BrowseRentalsHandler(IRentalReader rentalReader, IClock clock)
    {
        _rentalReader = rentalReader;
        _clock = clock;
    }

    public async Task<IEnumerable<RentalDto>> HandleAsync(BrowseRentals query, CancellationToken cancellationToken = default)
    {
        var today = _clock.Current().Date;
        var status = ResolveStatuses(query);

        var filter = new RentalReadFilter
        {
            CustomerId = query.CustomerId,
            ReservationId = query.ReservationId,
            Statuses = status,
            StartsOnOrAfter = query.From?.Date,
            EndsOnOrBefore = query.LifecycleState == RentalLifecycleState.Overdue
                ? today.AddDays(-1)
                : query.To?.Date
        };

        var rentals = await _rentalReader.GetAsync(filter, cancellationToken);

        return rentals.ToRentalDtos();
    }

    private static IReadOnlyCollection<RentalStatus>? ResolveStatuses(BrowseRentals query)
    {
        if (query.LifecycleState == RentalLifecycleState.Active ||
            query.LifecycleState == RentalLifecycleState.Overdue)
        {
            return [RentalStatus.Active];
        }

        if (query.LifecycleState == RentalLifecycleState.Closed)
            return [RentalStatus.Closed];

        return query.Status.HasValue ? [query.Status.Value] : null;
    }
}
