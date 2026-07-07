using GearFlow.Modules.Catalog.Application.Readers;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Application.Services;

public sealed class CatalogBrowsingService : ICatalogBrowsingService
{
    private const int MaxPageSize = 100;
    private readonly ICatalogBrowsingReader _catalogBrowsingReader;

    public CatalogBrowsingService(ICatalogBrowsingReader catalogBrowsingReader)
    {
        _catalogBrowsingReader = catalogBrowsingReader;
    }

    public async Task<PagedResult<PublicCatalogOfferDto>> BrowseOffersAsync(BrowseCatalogOffersInput input, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(input);

        return await _catalogBrowsingReader.BrowseOffersAsync(normalized, cancellationToken);
    }

    public async Task<PublicCatalogOfferDto> GetOfferAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var offer = await _catalogBrowsingReader.GetOfferAsync(variantId, cancellationToken);

        if (offer is null)
            throw new NotFoundException("Catalog offer not found.");

        return offer;
    }

    private static BrowseCatalogOffersInput Normalize(BrowseCatalogOffersInput input)
        => input with
        {
            Type = NormalizeOptional(input.Type),
            Brand = NormalizeOptional(input.Brand),
            Model = NormalizeOptional(input.Model),
            Size = NormalizeOptional(input.Size),
            Currency = CurrencyCode.From(input.Currency).Value,
            Page = Math.Max(1, input.Page),
            PageSize = Math.Clamp(input.PageSize, 1, MaxPageSize)
        };

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
