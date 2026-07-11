using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Queries.GetReservationDraft;

public sealed record GetReservationDraftHandler : IQueryHandler<GetReservationDraft, ReservationDraftDto?>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly IClock _clock;

    public GetReservationDraftHandler(IReservationRepository reservationRepository, IReservationAuthorizationService reservationAuthorizationService, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _reservationAuthorizationService = reservationAuthorizationService; 
        _clock = clock;
    }

    public async Task<ReservationDraftDto?> HandleAsync(GetReservationDraft query, CancellationToken cancellationToken)
    {
        var now = _clock.Current();
        var draft = await _reservationRepository.GetAsync(query.DraftId, cancellationToken);
        if (draft == null)
            throw new ReservationNotFoundException(query.DraftId);

        _reservationAuthorizationService.Authorize(draft);

        var isExpired = draft.IsDraftExpired(now);

        return new ReservationDraftDto
        (
            draft.Id,
            draft.CustomerId,
            draft.Status.ToString(),
            draft.ReservedPeriod.Start,
            draft.ReservedPeriod.End,
            draft.TtlExpiresAt,
            isExpired,
            draft.Currency.ToString(),
            draft.TotalPrice.Amount,
            draft.ReservationLines.Select(ri => new ReservedItemDto
            (
                ri.Id,
                ri.Item.VariantId,
                ri.Item.Model,
                ri.Item.Brand,
                ri.Item.UnitPrice.Amount,
                ri.LineTotalPrice.Amount
            )).ToList()
        );
    }
}
