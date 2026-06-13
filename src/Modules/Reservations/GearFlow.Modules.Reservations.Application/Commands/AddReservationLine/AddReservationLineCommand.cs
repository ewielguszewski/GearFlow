using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;

public sealed record AddReservationLineCommand(
    Guid ReservationId,
    Guid ReservationLineId,
    Guid OfferVariantId
    ) : ICommand;
