using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;

public sealed record AddReservationLineCommand(
    Guid ReservationLineId,
    Guid OfferVariantId,
    Guid? TargetCustomerId
    ) : ICrossModuleCommand;
