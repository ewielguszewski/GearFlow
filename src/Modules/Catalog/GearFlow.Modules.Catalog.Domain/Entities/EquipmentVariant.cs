using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Domain.Entities;


// Variant represents a customer-facing choice under a model.
// Type-specific size validation can be introduced later through an EquipmentSize value object, based on EquipmentModel's type
public class EquipmentVariant
{
    public Guid Id { get; private set; }
    public Guid EquipmentModelId { get; private set; }
    public EquipmentModel EquipmentModel { get; private set; } = default!;

    public string? DisplayName { get; private set; }
    public string? PublicNote { get; private set; }
    public Money? PriceOverride { get; private set; }

    public string? Size { get; private set; }

    public bool IsIndividuallyPriced => PriceOverride != null;


    private EquipmentVariant() { } // EF

    private EquipmentVariant(Guid equipmentModelId, string? displayName, string? publicNote, Money? priceOverride, string? size)
    {
        Id = Guid.NewGuid();
        EquipmentModelId = equipmentModelId;
        DisplayName = displayName;
        PublicNote = publicNote;
        PriceOverride = priceOverride;
        Size = size;
    }

    public static EquipmentVariant CreateStandard(Guid equipmentModelId, string? size)
        => new EquipmentVariant(equipmentModelId, null, null, null, size);

    public static EquipmentVariant CreatePremium(Guid equipmentModelId, string displayName, string publicNote, Money priceOverride, string? size)
        => new EquipmentVariant(equipmentModelId, displayName, publicNote, priceOverride, size);

    public void ChangePriceOverride(Money newPrice)
    => PriceOverride = newPrice;

    public void ClearPriceOverride()
        => PriceOverride = null;
}

