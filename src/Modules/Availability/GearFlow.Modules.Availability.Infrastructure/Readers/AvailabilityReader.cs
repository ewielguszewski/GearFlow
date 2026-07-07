using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Availability.Infrastructure.DAL;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Availability.Infrastructure.Readers;

internal sealed class AvailabilityReader : IAvailabilityReader
{
    private readonly AvailabilityDbContext _dbContext;

    public AvailabilityReader(AvailabilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableCountsByVariantAsync(
        IReadOnlyCollection<VariantAvailabilityCandidate> candidates,
        DateRange period,
        CancellationToken cancellationToken)
    {
        var itemIds = candidates
            .SelectMany(x => x.ActiveItemIds)
            .Distinct()
            .ToArray();

        if (itemIds.Length == 0)
            return candidates.ToDictionary(x => x.VariantId, _ => 0);

        var blockedItemIds = await _dbContext.Bookings
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.ItemId)
                        && x.TimePeriod.Start <= period.End
                        && x.TimePeriod.End >= period.Start)
            .Select(x => x.ItemId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var blocked = blockedItemIds.ToHashSet();

        return candidates.ToDictionary(
            x => x.VariantId,
            x => x.ActiveItemIds.Count(itemId => !blocked.Contains(itemId)));
    }
}