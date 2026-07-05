using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Availability.Core.Entities;
using GearFlow.Modules.Availability.Infrastructure.DAL;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Availability.Infrastructure.Allocators;

internal sealed class AvailabilityAllocator : IAvailabilityAllocator
{
    private readonly AvailabilityDbContext _dbContext;

    public AvailabilityAllocator(AvailabilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> TryAllocateItemAsync(IEnumerable<Guid> itemIds, Guid variantId, Guid sourceId, DateRange timePeriod, CancellationToken cancellationToken = default)
    {
        var candidates = itemIds
            .Distinct()
            .ToArray();

        if (candidates.Length == 0)
            return null;

        var blockedItemIds = await _dbContext.Bookings
            .Where(x => candidates.Contains(x.ItemId)
                        && x.TimePeriod.Start <= timePeriod.End
                        && x.TimePeriod.End >= timePeriod.Start)
            .Select(x => x.ItemId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var blocked = blockedItemIds.ToHashSet();
        var selectedItemId = candidates.FirstOrDefault(itemId => !blocked.Contains(itemId)); // later may be improved with some allocation strategy, shuffle, etc.

        if (selectedItemId == Guid.Empty)
            return null;

        var booking = ItemBooking.Create(selectedItemId, variantId, timePeriod, sourceId, BookingType.Reservation);

        await _dbContext.Bookings.AddAsync(booking, cancellationToken);

        return selectedItemId;
    }

    public async Task ReleaseReservationAllocationsAsync(Guid sourceId, CancellationToken cancellationToken = default)
    {
        var bookings = await _dbContext.Bookings
            .Where(x => x.Source == BookingType.Reservation && x.SourceId == sourceId)
            .ToArrayAsync(cancellationToken);

        _dbContext.Bookings.RemoveRange(bookings);
    }
}