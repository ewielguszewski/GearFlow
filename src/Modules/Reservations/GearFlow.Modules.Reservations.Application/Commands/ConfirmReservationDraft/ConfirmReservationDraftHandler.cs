using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Commands.ConfirmReservationDraft;

public sealed class ConfirmReservationDraftHandler : ICommandHandler<ConfirmReservationDraftCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IClock _clock;

    public ConfirmReservationDraftHandler(IReservationRepository reservationRepository, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _clock = clock;
    }

    public async Task HandleAsync(ConfirmReservationDraftCommand command, CancellationToken cancellationToken)
    {
        var now = _clock.Current();

        var draft = await _reservationRepository.GetAsync(command.draftId, cancellationToken);
        if (draft == null)
            throw new ReservationNotFoundException(command.draftId);

        if (!Enum.TryParse<PaymentMethod>(command.PaymentMethod, true, out var paymentMethod))
            throw new DomainException("Invalid payment method");

        draft.MarkAsPendingPayment(paymentMethod, now);

        draft.MarkAsConfirmed();
    }
}
