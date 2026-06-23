using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Modules.Catalog.Infrastructure.DAL;
using GearFlow.Shared.Abstractions.ValueObjects;
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
        var offer = await _dbContext.EquipmentVariants
            .AsNoTracking()
            .Where(v => v.Id == offerVariantId && v.EquipmentModel.IsPublished)
            .Select(v => new ReservableOfferDto
            (
                v.Id,
                v.DisplayName,
                v.EquipmentModel.Brand,
                v.EquipmentModel.Model,
                v.PublicNote,
                v.EquipmentModel.BasePrice!,
                v.OverriddenPrice,
                v.Size,
                Array.Empty<Guid>()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (offer is null)
            return null;

        var activeItemIds = await GetActiveItemIdsAsync([offerVariantId], cancellationToken);

        return offer with
        {
            ActiveItemIds = GetItemIds(activeItemIds, offerVariantId)
        };
    }

    public async Task<IReadOnlyCollection<CatalogOfferCandidateDto>> SearchOfferCandidatesAsync(OfferSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var offers = await ApplyFilters(BuildPublicOffersQuery(), criteria)
        .OrderBy(x => x.Brand)
        .ThenBy(x => x.Model)
        .ThenBy(x => x.VariantId)
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
                x.OverriddenPrice ?? x.BasePrice,
                x.Size,
                GetItemIds(activeItemIds, x.VariantId)))
            .ToArray();
    }

    private IQueryable<PublicOfferRow> BuildPublicOffersQuery()
        => _dbContext.EquipmentVariants
             .AsNoTracking()
             .Where(x => x.EquipmentModel.IsPublished)
             .Select(v => new PublicOfferRow(
                 v.Id,
                 v.EquipmentModel.Brand,
                 v.EquipmentModel.Model,
                 v.EquipmentModel.Type,
                 v.EquipmentModel.BasePrice!,
                 v.OverriddenPrice,
                 v.Size,
                 v.PublicNote));


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

    private static IQueryable<PublicOfferRow> ApplyFilters(IQueryable<PublicOfferRow> query, OfferSearchCriteria criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.Type))
        {
            if (!Enum.TryParse<EquipmentModelType>(
                    criteria.Type,
                    ignoreCase: true,
                    out var type))
            {
                return query.Where(_ => false);
            }

            query = query.Where(x => x.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Brand))
            query = query.Where(x => x.Brand == criteria.Brand.Trim());

        if (!string.IsNullOrWhiteSpace(criteria.Model))
            query = query.Where(x => x.Model == criteria.Model.Trim());

        if (!string.IsNullOrWhiteSpace(criteria.Size))
            query = query.Where(x => x.Size != null && x.Size == criteria.Size.Trim());

        if (criteria.MinPrice is { } min)
        {
            query = query.Where(x =>
                (x.OverriddenPrice != null ?
                x.OverriddenPrice.Currency.Value
                : x.BasePrice.Currency.Value) == min.Currency.Value
                &&
                (x.OverriddenPrice != null ?
                x.OverriddenPrice.Amount
                : x.BasePrice.Amount) >= min.Amount);
        }

        if (criteria.MaxPrice is { } max)
        {
            query = query.Where(x =>
                (x.OverriddenPrice != null ?
                x.OverriddenPrice.Currency.Value
                : x.BasePrice.Currency.Value) == max.Currency.Value
                &&
                (x.OverriddenPrice != null ?
                x.OverriddenPrice.Amount
                : x.BasePrice.Amount) <= max.Amount);
        }

        return query;
    }

    private sealed record PublicOfferRow(
        Guid VariantId,
        string Brand,
        string Model,
        EquipmentModelType Type,
        Money BasePrice,
        Money? OverriddenPrice,
        string? Size,
        string? PublicNote);
}
