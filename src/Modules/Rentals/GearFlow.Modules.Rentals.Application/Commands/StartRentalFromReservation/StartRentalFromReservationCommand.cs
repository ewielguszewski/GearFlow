using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Rentals.Application.Commands.StartRentalFromReservation;

public sealed record StartRentalFromReservationCommand(
    Guid RentalId,
    Guid ReservationId,
    IReadOnlyCollection<RentalLinePickupInput> Lines) : ICrossModuleCommand;

public sealed record RentalLinePickupInput(
    Guid ReservationLineId,
    ItemCondition Condition,
    string? ConditionNote);
