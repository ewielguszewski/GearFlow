namespace GearFlow.Modules.Catalog.Contracts;

public interface ICatalogOfferReader
{
    Task<ReservableOfferVariantDto?> GetReservableOfferVariantAsync(Guid offerVariantId, CancellationToken cancellationToken = default);
}
