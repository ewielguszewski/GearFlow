using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Application.Queries.Filters;
using GearFlow.Modules.Reservations.Application.Queries.Mappers;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Queries.GetUpcomingReservationsForUser;

public class GetUpcomingReservationsHandler : IQueryHandler<GetUpcomingReservations, IEnumerable<UpcomingReservationDto>>
{
    private readonly IReservationReader _reservationReader;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly IClock _clock;

    public GetUpcomingReservationsHandler(IReservationReader reservationReader, IReservationAuthorizationService reservationAuthorizationService, IClock clock)
    {
        _reservationReader = reservationReader;
        _reservationAuthorizationService = reservationAuthorizationService;
        _clock = clock;
    }

    public async Task<IEnumerable<UpcomingReservationDto>> HandleAsync(GetUpcomingReservations query, CancellationToken cancellationToken)
    {
        var customerId = _reservationAuthorizationService.ResolveCustomerId(query.TargetCustomerId);
        var today = _clock.Current().Date;

        var filter = new ReservationReadFilter
        {
            CustomerId = customerId,
            Statuses = [ReservationStatus.Confirmed],
            EndsOnOrAfter = today
        };

        var reservations = await _reservationReader.GetAsync(filter, cancellationToken);

        return reservations.ToUpcomingReservationDtos();
    }
}
