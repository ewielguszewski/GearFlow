using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Rentals.Domain.ValueObjects;

public sealed record RentalLinePickup(
    Guid RentalLineId,
    Guid ReservationLineId,
    ItemSnapshot Item,
    Money LineTotalPrice,
    ItemCondition PickupCondition,
    string? PickupConditionNote);
