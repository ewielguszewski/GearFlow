namespace GearFlow.Shared.Abstractions.ValueObjects;

public record struct CurrencyCode
{
    public string Value { get; }

    private CurrencyCode(string value)
    {
        Value = value;
    }

    public static CurrencyCode From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new MoneyException("Currency code cannot be empty.", nameof(value));

        if (value.Length != 3)
            throw new MoneyException("Currency code must have 3 characters.", nameof(value));

        return new CurrencyCode(value.ToUpperInvariant());
    }

    public static CurrencyCode PLN => new("PLN");


    public override string ToString() => Value;
}
