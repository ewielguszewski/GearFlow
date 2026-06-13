using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Reservations.Application.Exceptions;

public class OfferVariantNotAvailableException : AppException
{
    public Guid OfferVariantId { get; }

    public OfferVariantNotAvailableException(Guid offerVariantId) : base($"Offer variant with id {offerVariantId} is not available.")
    {
        OfferVariantId = offerVariantId;
    }
}
