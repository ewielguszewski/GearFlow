using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Rentals.Application.Commands.CompleteRentalReturn;

public sealed record CompleteRentalReturnCommand(
    Guid RentalId,
    IReadOnlyCollection<RentalLineReturnInput> Lines,
    decimal LateFeeAmount) : ICommand;

public sealed record RentalLineReturnInput(
    Guid RentalLineId,
    ItemCondition Condition,
    string? Note,
    decimal DamageFeeAmount);
