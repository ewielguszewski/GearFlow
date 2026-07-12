using GearFlow.Modules.Reservations.Domain.Entities;

namespace GearFlow.Modules.Reservations.Application.Queries.Filters;

public sealed record ReservationReadFilter
{
    public Guid? CustomerId { get; init; }

    public IReadOnlyCollection<ReservationStatus>? Statuses { get; init; }
    public CancellationReason? CancReason { get; init; }

    public DateTime? StartsOnOrAfter { get; init; }
    public DateTime? StartsOnOrBefore { get; init; }

    public DateTime? EndsOnOrAfter { get; init; }
    public DateTime? EndsOnOrBefore { get; init; }
}
