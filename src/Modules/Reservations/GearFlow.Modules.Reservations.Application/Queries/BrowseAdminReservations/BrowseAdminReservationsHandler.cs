using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Application.Queries.Filters;
using GearFlow.Modules.Reservations.Application.Queries.Mappers;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Queries.BrowseAdminReservations;

public sealed class BrowseAdminReservationsHandler : IQueryHandler<BrowseAdminReservations, IEnumerable<AdminReservationDto>>
{
    private readonly IReservationReader _reservationReader;
    private readonly IClock _clock;

    public BrowseAdminReservationsHandler(IReservationReader reservationReader, IClock clock)
    {
        _reservationReader = reservationReader;
        _clock = clock;
    }

    public async Task<IEnumerable<AdminReservationDto>> HandleAsync(BrowseAdminReservations query, CancellationToken cancellationToken)
    {
        var today = _clock.Current().Date;

        var filter = new ReservationReadFilter
        {
            CustomerId = query.CustomerId,
            Statuses = query.Status.HasValue ? [query.Status.Value] : null,
            CancReason = query.CancellationReason,
            StartsOnOrAfter = query.From?.Date,
            StartsOnOrBefore = query.PickupState == ReservationPickupState.Overdue ? today.AddDays(-1) : null,
            EndsOnOrAfter = query.PickupState == ReservationPickupState.Upcoming ? today : null,
            EndsOnOrBefore = query.To?.Date
        };

        var reservations = await _reservationReader.GetAsync(filter, cancellationToken);

        return reservations.ToAdminReservationDtos();
    }
}
