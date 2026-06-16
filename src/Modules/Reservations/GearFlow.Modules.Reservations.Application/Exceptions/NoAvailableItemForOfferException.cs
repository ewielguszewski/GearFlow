using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Reservations.Application.Exceptions;

public class NoAvailableItemForOfferException : AppException
{
    public Guid OfferVariantId { get; }

    public NoAvailableItemForOfferException(Guid offerVariantId)
        : base($"No available item for offer variant with id: {offerVariantId}")
    {
        OfferVariantId = offerVariantId;
    }
}
