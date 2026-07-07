using GearFlow.Modules.Catalog.Application.Readers;
using GearFlow.Modules.Catalog.Application.Services;
using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Catalog.Infrastructure.DAL;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Catalog.Infrastructure.Readers;

internal sealed class CatalogBrowsingReader : ICatalogBrowsingReader
{
    private readonly CatalogDbContext _dbContext;

    public CatalogBrowsingReader(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PublicCatalogOfferDto>> BrowseOffersAsync(BrowseCatalogOffersInput input, CancellationToken cancellationToken = default)
    {
        var criteria = ToCriteria(input);
        var query = CatalogOfferQuery.ApplyFilters(
            CatalogOfferQuery.BuildPublishedOffers(_dbContext),
            criteria);

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await CatalogOfferQuery.SelectRows(
                CatalogOfferQuery.OrderByCatalog(query)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<PublicCatalogOfferDto>(
            rows.Select(Map).ToArray(),
            input.Page,
            input.PageSize,
            totalCount);
    }

    public async Task<PublicCatalogOfferDto?> GetOfferAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var row = await CatalogOfferQuery.SelectRows(
                CatalogOfferQuery.BuildPublishedOffers(_dbContext)
                    .Where(x => x.Id == variantId))
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : Map(row);
    }

    private static OfferSearchCriteria ToCriteria(BrowseCatalogOffersInput input)
    {
        var currency = CurrencyCode.From(input.Currency);

        return new OfferSearchCriteria
        {
            Type = input.Type,
            Brand = input.Brand,
            Model = input.Model,
            Size = input.Size,
            MinPrice = input.MinPrice.HasValue ? Money.Create(input.MinPrice.Value, currency) : null,
            MaxPrice = input.MaxPrice.HasValue ? Money.Create(input.MaxPrice.Value, currency) : null
        };
    }

    private static PublicCatalogOfferDto Map(CatalogOfferRow row)
        => new(
            row.VariantId,
            row.ModelId,
            row.Brand,
            row.Model,
            row.Type.ToString(),
            row.Size,
            row.DisplayName,
            row.PublicNote,
            row.Price.Amount,
            row.Price.Currency.Value,
            row.OverriddenPrice is not null);
}
