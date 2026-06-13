using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;

public class CreateDraftReservationHandler : ICommandHandler<CreateDraftReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;

    public CreateDraftReservationHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task HandleAsync(CreateDraftReservationCommand command, CancellationToken cancellationToken = default)
    {
        var period = new DateRange(command.From, command.To);
        var currency = CurrencyCode.From(command.Currency);

        var reservation = Reservation.CreateDraft(command.Id, command.CustomerId, period, currency);

        await _reservationRepository.AddAsync(reservation, cancellationToken);
    }
}
