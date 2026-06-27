using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;

public class CreateDraftReservationHandler : ICommandHandler<CreateDraftReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAvailabilityAllocator _availabilityAllocator;

    public CreateDraftReservationHandler(IReservationRepository reservationRepository, IAvailabilityAllocator availabilityAllocator)
    {
        _reservationRepository = reservationRepository;
        _availabilityAllocator = availabilityAllocator;
    }

    public async Task HandleAsync(CreateDraftReservationCommand command, CancellationToken cancellationToken = default)
    {
        var period = new DateRange(command.From, command.To);
        var currency = CurrencyCode.From(command.Currency);

        var existingDraftReservation = await _reservationRepository.GetDraftByCustomerIdAsync(command.CustomerId, cancellationToken);
        if (existingDraftReservation != null)
        {
            if (existingDraftReservation.ReservationLines.Any())
                await _availabilityAllocator.ReleaseReservationAllocationsAsync(existingDraftReservation.Id, cancellationToken);

            existingDraftReservation.CancelReservation(CancellationReason.ReplacedByNewDraft);

            _reservationRepository.Update(existingDraftReservation);
        }

        var reservation = Reservation.CreateDraft(command.Id, command.CustomerId, period, currency);

        _reservationRepository.Add(reservation);
    }
}

//  todo: Add unique constraint on db when implementing DAL
