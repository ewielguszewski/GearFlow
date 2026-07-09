using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.Options;

namespace GearFlow.Modules.Reservations.Infrastructure.Background;

public sealed class ExpiredDraftReservationProcessor : IExpiredDraftReservationProcessor
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IAvailabilityAllocator _availabilityAllocator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IOptions<ReservationExpiryCleanupOptions> _options;

    public ExpiredDraftReservationProcessor(
        IReservationRepository reservationRepository,
        IAvailabilityAllocator availabilityAllocator,
        IUnitOfWork unitOfWork,
        IClock clock,
        IOptions<ReservationExpiryCleanupOptions> options)
    {
        _reservationRepository = reservationRepository;
        _availabilityAllocator = availabilityAllocator;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _options = options;
    }

    public async Task<int> ProcessExpiredDraftsAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.Current();
        var processedCount = 0;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var expiredDrafts = await _reservationRepository.GetExpiredDraftsAsync(now, _options.Value.BatchSize, cancellationToken);

            foreach (var draft in expiredDrafts)
            {
                await _availabilityAllocator.ReleaseReservationAllocationsAsync(draft.Id, cancellationToken);
                draft.Expire(now);
                processedCount++;
            }
        }, cancellationToken);

        return processedCount;
    }
}