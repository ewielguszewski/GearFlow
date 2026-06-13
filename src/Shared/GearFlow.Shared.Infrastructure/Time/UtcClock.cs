using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Shared.Infrastructure.Time;

public sealed class UtcClock : IClock
{
    public DateTime Current() => DateTime.UtcNow;
}
