using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Reservations.Application.Exceptions;

public class OfferNotAvailableException : AppException
{
    public Guid OfferVariantId { get; }

    public OfferNotAvailableException(Guid offerVariantId) : base($"Offer variant with id {offerVariantId} is not available.")
    {
        OfferVariantId = offerVariantId;
    }
}
