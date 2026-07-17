using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Rentals.Domain.ValueObjects;

public sealed record RentalLineReturn(
    Guid RentalLineId,
    ItemCondition ReturnCondition,
    string? ReturnNote,
    Money DamageFee);
