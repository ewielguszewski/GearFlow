using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.Security;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Commands.ConfirmReservationDraft;

public sealed class ConfirmReservationDraftHandler : ICommandHandler<ConfirmReservationDraftCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly IClock _clock;

    public ConfirmReservationDraftHandler(IReservationRepository reservationRepository, IReservationAuthorizationService reservationAuthorizationService, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _reservationAuthorizationService = reservationAuthorizationService;
        _clock = clock;
    }

    public async Task HandleAsync(ConfirmReservationDraftCommand command, CancellationToken cancellationToken)
    {
        var now = _clock.Current();

        var customerId = _reservationAuthorizationService.ResolveCustomerId(command.TargetCustomerId);
        var draft = await _reservationRepository.GetDraftByCustomerIdAsync(customerId, cancellationToken);
        if (draft == null)
            throw new ReservationNotFoundException(null);

        _reservationAuthorizationService.Authorize(draft);

        draft.MarkAsPendingPayment(command.PaymentMethod, now);

        draft.MarkAsConfirmed();
    }
}
