using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Reservations.Application.Exceptions;

public class NoAvailableItemForOfferVariantException : AppException
{
    public Guid OfferVariantId { get; }

    public NoAvailableItemForOfferVariantException(Guid offerVariantId)
        : base($"No available item for offer variant with id: {offerVariantId}")
    {
        OfferVariantId = offerVariantId;
    }
}
