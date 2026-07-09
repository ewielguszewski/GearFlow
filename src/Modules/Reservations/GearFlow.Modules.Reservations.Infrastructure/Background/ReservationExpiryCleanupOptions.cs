namespace GearFlow.Modules.Reservations.Infrastructure.Background;

public sealed class ReservationExpiryCleanupOptions
{
    public const string SectionName = "reservations:expiryCleanup";

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
    public int BatchSize { get; set; } = 100;
    public int InitialDelaySeconds { get; set; } = 5;
}
