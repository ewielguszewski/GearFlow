using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Contracts;

public class OfferSearchCriteria
{
    public string? Type { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public Money? MinPrice { get; set; }
    public Money? MaxPrice { get; set; }
    public string? Size { get; set; }
}
