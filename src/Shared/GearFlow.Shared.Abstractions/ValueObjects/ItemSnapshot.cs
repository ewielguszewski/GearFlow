namespace GearFlow.Shared.Abstractions.ValueObjects;

// Snapshot used for historical display and navigation.
// VariantId point to the public catalog path; ItemId points to the concrete item held internally.
public readonly record struct ItemSnapshot
{
    public Guid ItemId { get; init; }
    public Guid VariantId { get; init; }
    public string? VariantName { get; init; }
    public string Brand { get; init; }
    public string Model { get; init; }
    public string? PublicNote { get; init; }
    public Money UnitPrice { get; init; }
    public PriceSource PriceSource { get; init; }
    public string? Size { get; init; }

    public static ItemSnapshot Create(
        Guid itemId,
        Guid variantId,
        string? variantName,
        string brand,
        string model,
        string? publicNote,
        Money unitPrice,
        PriceSource priceSource,
        string? size
        ) => new() 
        {
            ItemId = itemId,
            VariantId = variantId,
            VariantName = variantName,
            Brand = brand,
            Model = model,
            PublicNote = publicNote,
            UnitPrice = unitPrice,
            PriceSource = priceSource,
            Size = size
        };
}

public enum PriceSource
{
    CatalogModel,
    CatalogVariantOverride,
}