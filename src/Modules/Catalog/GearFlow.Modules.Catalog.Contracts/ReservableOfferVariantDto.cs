using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Contracts;

public class ReservableOfferVariantDto
{
    public Guid VariantId { get; set; }
    public string? VariantName { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string? PublicNote { get; set; }
    public Money BasePrice { get; set; }
    public Money? OverridenPrice { get; set; }
    public string? Size { get; set; } = null!;
}