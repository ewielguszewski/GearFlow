using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Domain.Entities;

public class EquipmentItem
{
    public Guid Id { get; private set; }
    public Guid EquipmentVariantId { get; private set; }
    public EquipmentVariant EquipmentVariant { get; private set; } = default!;

    public string AssetTag { get; private set; } = default!;
    public string? InternalNote { get; private set; }

    public EquipmentItemStatus Status { get; private set; }


    // Catalog stores current inspection facts for fast rentability checks.
    // Full maintenance history and scheduled service windows belong to the Maintenance module.
    public DateTime? LastMaintenanceAt { get; private set; }
    public DateTime NextInspectionAt { get; private set; }
    public DateTime CreatedAt { get; }

    public bool IsRentable => Status == EquipmentItemStatus.Active;


    private EquipmentItem() { } // EF

    private EquipmentItem(Guid equipmentVariantId, string assetTag, string? internalNote, EquipmentItemStatus status)
    {
        Id = Guid.NewGuid();
        EquipmentVariantId = equipmentVariantId;
        AssetTag = assetTag;
        InternalNote = internalNote;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        NextInspectionAt = CreatedAt.AddMonths(3); // Later this should come from an inspection policy.
    }

    public static EquipmentItem Create(Guid equipmentVariantId, string assetTag, string? internalNote, EquipmentItemStatus status)
        => new EquipmentItem(equipmentVariantId, assetTag, internalNote, status);



    public void MarkAsUnavailable()
        => Status = EquipmentItemStatus.Unavailable;

    public void MarkAsRequiresInspection()
        => Status = EquipmentItemStatus.RequiresInspection;

    public void MarkAsBroken()
        => Status = EquipmentItemStatus.Broken;

    public void MarkAsLost()
        => Status = EquipmentItemStatus.Lost;

    public void Retire()
        => Status = EquipmentItemStatus.Retired;

    public void MarkAsActive()
        => Status = EquipmentItemStatus.Active;

    public void EnsureRentable()
    {
        if (!IsRentable)
            throw new DomainException("Equipment item is not rentable.");
    }

}
