using GearFlow.Shared.Abstractions.ValueObjects;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Modules.Catalog.Domain.Enums;

namespace GearFlow.Modules.Catalog.Domain.Entities;

public class EquipmentModel
{
    private readonly List<EquipmentVariant> _equipmentVariants = new();

    public Guid Id { get; private set; }
    public string Brand { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public Money? BasePrice { get; private set; }
    public bool IsPublished { get; private set; }
    public EquipmentModelType Type { get; private set; }

    public IReadOnlyCollection<EquipmentVariant> EquipmentVariants => _equipmentVariants;


    private EquipmentModel() { } // EF

    private EquipmentModel(string brand, string model, string? description, Money? basePrice, EquipmentModelType type)
    {
        Id = Guid.NewGuid();
        Brand = brand;
        Model = model;
        Slug = GenerateSlug(brand, model);
        Description = description;
        BasePrice = basePrice;
        Type = type;
    }

    public static EquipmentModel CreatePublished(string brand, string model, string description, Money basePrice, EquipmentModelType type)
    {
        var equipmentModel = new EquipmentModel(brand, model, description, basePrice, type);
        equipmentModel.Publish();

        return equipmentModel;
    }

    public static EquipmentModel CreateUnpublished(string brand, string model, EquipmentModelType type)
        => new EquipmentModel(brand, model, null, null, type);
    
    public EquipmentVariant AddStandardVariant(string size)
    {
        var equipmentVariant = EquipmentVariant.CreateStandard(Id, size);
        _equipmentVariants.Add(equipmentVariant);

        return equipmentVariant;
    }

    public EquipmentVariant AddPremiumVariant(string displayName, string publicNote, Money priceOverride, string? size)
    {
        var equipmentVariant = EquipmentVariant.CreatePremium(Id, displayName, publicNote, priceOverride, size);
        _equipmentVariants.Add(equipmentVariant);
        
        return equipmentVariant;
    }

    public void ChangePublicDetails(string brand, string model, string description)
    {
        Brand = brand;
        Model = model;
        Description = description;
        Slug = GenerateSlug(brand, model);
    }

    public void ChangeBasePrice(Money basePrice)
        => BasePrice = basePrice;

    public void Publish()
    {
        if (string.IsNullOrWhiteSpace(Description) || BasePrice == null)
            throw new DomainException("Equipment model cannot be published without description and base price.");

        IsPublished = true;
    }

    public void Unpublish()
    {
        IsPublished = false;
    }
 
    private string GenerateSlug(string brand, string model)
        => $"{brand}-{model}".ToLowerInvariant().Replace(" ", "-");
}
