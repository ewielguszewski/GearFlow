using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Application.Queries.Mappers;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Queries.GetCurrentReservationDraft;

public sealed record GetCurrentReservationDraftHandler : IQueryHandler<GetCurrentReservationDraft, ReservationDraftDto?>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly IClock _clock;

    public GetCurrentReservationDraftHandler(IReservationRepository reservationRepository, IReservationAuthorizationService reservationAuthorizationService, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _reservationAuthorizationService = reservationAuthorizationService; 
        _clock = clock;
    }
    public async Task<ReservationDraftDto?> HandleAsync(GetCurrentReservationDraft query, CancellationToken cancellationToken)
    {
        var now = _clock.Current();
        var customerId = _reservationAuthorizationService.ResolveCustomerId(query.TargetCustomerId);

        var draft = await _reservationRepository.GetDraftByCustomerIdAsync(customerId, cancellationToken);
        if (draft == null)
            return null;

        _reservationAuthorizationService.Authorize(draft);

        var isExpired = draft.IsDraftExpired(now);

        return draft.ToReservationDraftDto(isExpired);
    }
}