using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Commands.RemoveReservationLine;

public class RemoveReservationLineHandler : ICommandHandler<RemoveReservationLineCommand> 
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly IAvailabilityAllocator _availabilityAllocator;
    private readonly IClock _clock;

    public RemoveReservationLineHandler(IReservationRepository reservationRepository, IAvailabilityAllocator availabilityAllocator, 
        IReservationAuthorizationService reservationAuthorizationService, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _reservationAuthorizationService = reservationAuthorizationService;
        _availabilityAllocator = availabilityAllocator;
        _clock = clock;
    }

    public async Task HandleAsync(RemoveReservationLineCommand command, CancellationToken cancellationToken)
    {
        var now = _clock.Current();

        var customerId = _reservationAuthorizationService.ResolveCustomerId(command.TargetCustomerId);
        var draft = await _reservationRepository.GetDraftByCustomerIdAsync(customerId, cancellationToken);
        if (draft == null)
            throw new ReservationNotFoundException(null);

        _reservationAuthorizationService.Authorize(draft);

        var line = draft.ReservationLines.SingleOrDefault(x => x.Id == command.LineId);
        if (line == null)
            return;

        draft.RemoveReservationLine(command.LineId, now);

        await _availabilityAllocator.ReleaseReservationItemAllocationAsync(draft.Id, line.Item.ItemId, cancellationToken);
    }
}
