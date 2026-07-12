using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Reservations.Application.Commands.RemoveReservationLine;

public sealed record RemoveReservationLineCommand(Guid? TargetCustomerId, Guid LineId) : ICrossModuleCommand;
