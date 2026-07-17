using GearFlow.Modules.Rentals.Application.Exceptions;
using GearFlow.Modules.Rentals.Domain.Entities;
using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Modules.Rentals.Domain.Repositories;
using GearFlow.Modules.Rentals.Domain.ValueObjects;
using GearFlow.Modules.Reservations.Contracts.Readers;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Rentals.Application.Commands.StartRentalFromReservation;

public sealed class StartRentalFromReservationHandler : ICommandHandler<StartRentalFromReservationCommand>
{
    private readonly IReservationRentalReader _reservationReader;
    private readonly IReservationRentalFulfillment _reservationFulfillment;
    private readonly IRentalRepository _rentalRepository;
    private readonly IClock _clock;

    public StartRentalFromReservationHandler(
        IReservationRentalReader reservationReader,
        IReservationRentalFulfillment reservationFulfillment,
        IRentalRepository rentalRepository,
        IClock clock)
    {
        _reservationReader = reservationReader;
        _reservationFulfillment = reservationFulfillment;
        _rentalRepository = rentalRepository;
        _clock = clock;
    }

    public async Task HandleAsync(StartRentalFromReservationCommand command, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationReader.GetConfirmedReservationAsync(command.ReservationId, cancellationToken)
                          ?? throw new ReservationForRentalNotFoundException(command.ReservationId);

        var existingRental = await _rentalRepository.GetByReservationIdAsync(command.ReservationId, cancellationToken);

        if (existingRental is not null)
            throw new RentalAlreadyExistsForReservationException(command.ReservationId);

        var pickupInputs = command.Lines.ToDictionary(x => x.ReservationLineId);

        if (pickupInputs.Count != command.Lines.Count)
            throw new DomainException("Pickup input contains duplicated reservation lines.");

        var reservationLineIds = reservation.Lines.Select(x => x.Id).ToHashSet();
        if (pickupInputs.Keys.Any(x => !reservationLineIds.Contains(x)))
            throw new DomainException("Pickup input contains reservation lines that do not belong to this reservation.");

        var now = _clock.Current();

        var rentalLines = reservation.Lines
            .Select(line =>
            {
                pickupInputs.TryGetValue(line.Id, out var pickup);

                return new RentalLinePickup(
                    Guid.NewGuid(),
                    line.Id,
                    ItemSnapshot.Create(
                        line.ItemId,
                        line.VariantId,
                        line.VariantName,
                        line.Brand,
                        line.Model,
                        line.PublicNote,
                        line.UnitPrice,
                        line.PriceSource,
                        line.Size),
                    line.LineTotalPrice,
                    pickup?.Condition ?? ItemCondition.Good,
                    pickup?.ConditionNote);
            })
            .ToArray();

        var rental = Rental.StartFromReservation(
            command.RentalId,
            reservation.Id,
            reservation.CustomerId,
            reservation.ReservedPeriod,
            reservation.Currency,
            rentalLines,
            now);

        _rentalRepository.Add(rental);
        await _reservationFulfillment.MarkAsFulfilledAsync(reservation.Id, now, cancellationToken);
    }
}
