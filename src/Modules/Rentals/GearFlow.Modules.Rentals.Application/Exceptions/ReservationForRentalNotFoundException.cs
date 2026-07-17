using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Rentals.Application.Exceptions;

public sealed class ReservationForRentalNotFoundException(Guid reservationId)
    : NotFoundException($"Confirmed reservation '{reservationId}' was not found.");
