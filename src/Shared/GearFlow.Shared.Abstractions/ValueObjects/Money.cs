using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Shared.Abstractions.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }


    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, CurrencyCode currency)
    {
        if (amount < 0)
            throw new MoneyException("Amount cannot be negative.", nameof(amount));

        var rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);

        return new Money(rounded, currency);
    }

    public static Money CreateFromPln(decimal amount)
        => Create(amount, CurrencyCode.PLN);

    public Money Add(Money other)
    {
        IsSameCurrency(other);

        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        IsSameCurrency(other);

        if (other.Amount > Amount)
            throw new MoneyException("Resulting amount cannot be negative");

        return Create(Amount - other.Amount, Currency);
    }

    public bool IsSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new MoneyException("Currency mismatch.");

        return true;
    }

    public bool IsZero() => Amount == 0;

    public override string ToString()
    {
        return $"{Amount} {Currency}";
    }
}

internal class MoneyException : DomainException
{
    public MoneyException(string message) : base(message)
    {
    }

    public MoneyException(string message, string paramName) : base(message, paramName)
    {
    }
}