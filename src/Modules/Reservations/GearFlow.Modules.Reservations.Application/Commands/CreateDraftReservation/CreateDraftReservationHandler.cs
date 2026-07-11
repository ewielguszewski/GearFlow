using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;

public class CreateDraftReservationHandler : ICommandHandler<CreateDraftReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAvailabilityAllocator _availabilityAllocator;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly IClock _clock;

    public CreateDraftReservationHandler(IReservationRepository reservationRepository, IAvailabilityAllocator availabilityAllocator, 
        IReservationAuthorizationService reservationAuthorizationService, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _availabilityAllocator = availabilityAllocator;
        _reservationAuthorizationService = reservationAuthorizationService;
        _clock = clock;
    }

    public async Task HandleAsync(CreateDraftReservationCommand command, CancellationToken cancellationToken = default)
    {
        var period = new DateRange(command.From.Date, command.To.Date);
        var currency = CurrencyCode.From(command.Currency);
        var now = _clock.Current();

        var customerId = _reservationAuthorizationService.ResolveCustomerId(command.CustomerId);

        var existingDraftReservation = await _reservationRepository.GetDraftByCustomerIdAsync(customerId, cancellationToken);
        if (existingDraftReservation != null)
        {
            if (existingDraftReservation.ReservationLines.Any())
                await _availabilityAllocator.ReleaseReservationAllocationsAsync(existingDraftReservation.Id, cancellationToken);

            existingDraftReservation.CancelReservation(CancellationReason.ReplacedByNewDraft);
        }

        var reservation = Reservation.CreateDraft(command.Id, customerId, period, currency, now);

        _reservationRepository.Add(reservation);
    }
}
