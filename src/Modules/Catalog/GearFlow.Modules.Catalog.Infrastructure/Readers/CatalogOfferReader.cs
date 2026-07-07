using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Modules.Catalog.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Catalog.Infrastructure.Readers;

internal sealed class CatalogOfferReader : ICatalogOfferReader
{
    private readonly CatalogDbContext _dbContext;

    public CatalogOfferReader(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReservableOfferDto?> GetReservableOfferAsync(Guid offerVariantId, CancellationToken cancellationToken = default)
    {
        var offer = await CatalogOfferQuery.SelectRows(
                CatalogOfferQuery.BuildPublishedOffers(_dbContext)
                    .Where(x => x.Id == offerVariantId))
            .FirstOrDefaultAsync(cancellationToken);

        if (offer is null)
            return null;

        var activeItemIds = await GetActiveItemIdsAsync([offerVariantId], cancellationToken);

        return new ReservableOfferDto(
            offer.VariantId,
            offer.DisplayName,
            offer.Brand,
            offer.Model,
            offer.PublicNote,
            offer.BasePrice,
            offer.OverriddenPrice,
            offer.Size,
            GetItemIds(activeItemIds, offer.VariantId));
    }

    public async Task<IReadOnlyCollection<CatalogOfferCandidateDto>> SearchOfferCandidatesAsync(OfferSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var offerQuery = CatalogOfferQuery.ApplyFilters(
                CatalogOfferQuery.BuildPublishedOffers(_dbContext),
                criteria);

        var offers = await CatalogOfferQuery.SelectRows(CatalogOfferQuery.OrderByCatalog(offerQuery))
            .ToArrayAsync(cancellationToken);

        if (offers.Length == 0)
            return [];

        var variantIds = offers
            .Select(x => x.VariantId)
            .ToArray();

        var activeItemIds = await GetActiveItemIdsAsync(
            variantIds,
            cancellationToken);

        return offers
            .Select(x => new CatalogOfferCandidateDto(
                x.VariantId,
                x.Brand,
                x.Model,
                x.Type.ToString(),
                x.Price,
                x.Size,
                GetItemIds(activeItemIds, x.VariantId)))
            .ToArray();
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<Guid>>> GetActiveItemIdsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken)
    {
        var activeItemGroups = await _dbContext.EquipmentItems
            .AsNoTracking()
            .Where(x => variantIds.Contains(x.EquipmentVariantId) && x.Status == EquipmentItemStatus.Active)
            .GroupBy(x => x.EquipmentVariantId)
            .Select(x => new
            {
                VariantId = x.Key,
                ItemIds = x.Select(item => item.Id).ToArray()
            })
            .ToDictionaryAsync(x => x.VariantId, x => (IReadOnlyCollection<Guid>)x.ItemIds, cancellationToken);

        return activeItemGroups;
    }

    private static IReadOnlyCollection<Guid> GetItemIds(IReadOnlyDictionary<Guid, IReadOnlyCollection<Guid>> itemIdsByVariant, Guid variantId)
        => itemIdsByVariant.TryGetValue(variantId, out var itemIds) ? itemIds : [];
}
