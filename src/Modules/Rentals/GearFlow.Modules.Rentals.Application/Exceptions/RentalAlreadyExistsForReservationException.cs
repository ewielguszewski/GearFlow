using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Rentals.Application.Exceptions;

public sealed class RentalAlreadyExistsForReservationException(Guid reservationId)
    : ConflictException($"Rental for reservation '{reservationId}' already exists.");
