namespace GearFlow.Modules.Catalog.Contracts;

public interface ICatalogOfferReader
{
    Task<ReservableOfferDto?> GetReservableOfferAsync(Guid offerVariantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CatalogOfferCandidateDto>> SearchOfferCandidatesAsync(OfferSearchCriteria criteria, CancellationToken cancellationToken = default);
}
