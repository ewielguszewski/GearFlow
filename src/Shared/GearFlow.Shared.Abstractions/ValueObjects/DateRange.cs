using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Shared.Abstractions.ValueObjects;

public record struct DateRange
{
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }


    public DateRange(DateTime start, DateTime end)
    {
        if (end < start)
            throw new DomainException("End date must be greater than or equal to start date.");

        Start = start;
        End = end;
    }

    public bool Overlaps(DateRange other)
        => Start < other.End && End > other.Start;
}
