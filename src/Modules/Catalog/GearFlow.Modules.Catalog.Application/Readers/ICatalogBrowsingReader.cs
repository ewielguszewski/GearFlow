using GearFlow.Modules.Catalog.Application.Services;

namespace GearFlow.Modules.Catalog.Application.Readers;

public interface ICatalogBrowsingReader
{
    Task<PagedResult<PublicCatalogOfferDto>> BrowseOffersAsync(BrowseCatalogOffersInput input, CancellationToken cancellationToken = default);
    Task<PublicCatalogOfferDto?> GetOfferAsync(Guid variantId, CancellationToken cancellationToken = default);
}
