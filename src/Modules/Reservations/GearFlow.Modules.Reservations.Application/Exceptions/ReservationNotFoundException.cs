using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Reservations.Application.Exceptions;

public class ReservationNotFoundException : NotFoundException
{
    public Guid ReservationId { get; }

    public ReservationNotFoundException(Guid reservationId)
        : base($"Reservation '{reservationId}' was not found.")
    {
        ReservationId = reservationId;
    }
}
