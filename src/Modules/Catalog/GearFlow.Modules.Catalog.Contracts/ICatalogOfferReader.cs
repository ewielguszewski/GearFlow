namespace GearFlow.Modules.Catalog.Contracts;

public interface ICatalogOfferReader
{
    Task<ReservableOfferVariantDto?> GetReservableOfferVariantAsync(Guid offerVariantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CatalogOfferCandidateDto>> SearchOfferCandidatesAsync(OfferSearchCriteria criteria, CancellationToken cancellationToken = default);
}
