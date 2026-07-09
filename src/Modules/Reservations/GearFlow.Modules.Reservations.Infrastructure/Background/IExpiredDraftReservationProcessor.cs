namespace GearFlow.Modules.Reservations.Infrastructure.Background;

public interface IExpiredDraftReservationProcessor
{
    Task<int> ProcessExpiredDraftsAsync(CancellationToken cancellationToken = default);
}
