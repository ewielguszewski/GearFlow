namespace GearFlow.Modules.Catalog.Application.Services;

public interface ICatalogBrowsingService
{
    Task<PagedResult<PublicCatalogOfferDto>> BrowseOffersAsync(BrowseCatalogOffersInput input, CancellationToken cancellationToken = default);
    Task<PublicCatalogOfferDto> GetOfferAsync(Guid variantId, CancellationToken cancellationToken = default);
}
