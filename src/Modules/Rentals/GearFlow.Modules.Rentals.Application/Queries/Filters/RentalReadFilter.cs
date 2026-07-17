using GearFlow.Modules.Rentals.Domain.Enums;

namespace GearFlow.Modules.Rentals.Application.Queries.Filters;

public sealed record RentalReadFilter
{
    public Guid? RentalId { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? ReservationId { get; init; }

    public IReadOnlyCollection<RentalStatus>? Statuses { get; init; }

    public DateTime? StartsOnOrAfter { get; init; }
    public DateTime? StartsOnOrBefore { get; init; }

    public DateTime? EndsOnOrAfter { get; init; }
    public DateTime? EndsOnOrBefore { get; init; }

    public DateTime? PickedUpOnOrAfter { get; init; }
    public DateTime? PickedUpOnOrBefore { get; init; }

    public DateTime? ReturnedOnOrAfter { get; init; }
    public DateTime? ReturnedOnOrBefore { get; init; }
}
