using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearFlow.Shared.Infrastructure.Postgres.Configurations;

public static class ValueObjectsMappingExtensions
{
    public static ComplexPropertyBuilder<Money> ConfigureMoney(this ComplexPropertyBuilder<Money> builder, string columnPrefix)
    {
        builder.Property(x => x.Amount)
            .HasColumnName($"{columnPrefix}_amount")
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasColumnName($"{columnPrefix}_currency")
            .HasConversion(
                currency => currency.Value,
                value => CurrencyCode.From(value))
            .HasMaxLength(3);

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, Money> ConfigureMoney<TEntity>(
        this OwnedNavigationBuilder<TEntity, Money> builder,
        string columnPrefix)
        where TEntity : class
    {
        builder.Property(x => x.Amount)
            .HasColumnName($"{columnPrefix}_amount")
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasColumnName($"{columnPrefix}_currency")
            .HasConversion(
                currency => currency.Value,
                value => CurrencyCode.From(value))
            .HasMaxLength(3);

        return builder;
    }

    public static ComplexPropertyBuilder<DateRange> ConfigureDateRange(this ComplexPropertyBuilder<DateRange> builder, string columnPrefix)
    {
        builder.Property(x => x.Start)
            .HasColumnName($"{columnPrefix}_from")
            .HasColumnType("date");

        builder.Property(x => x.End)
            .HasColumnName($"{columnPrefix}_to")
            .HasColumnType("date");

        return builder;
    }

    public static PropertyBuilder<CurrencyCode> ConfigureCurrencyCode(this PropertyBuilder<CurrencyCode> builder, string columnName)
    {
        return builder
            .HasColumnName(columnName)
            .HasConversion(
                currency => currency.Value,
                value => CurrencyCode.From(value))
            .HasMaxLength(3);
    }
}
