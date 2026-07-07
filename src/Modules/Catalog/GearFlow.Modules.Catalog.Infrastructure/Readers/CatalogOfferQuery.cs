using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Catalog.Domain.Entities;
using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Modules.Catalog.Infrastructure.DAL;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Catalog.Infrastructure.Readers;

internal static class CatalogOfferQuery
{
    public static IQueryable<EquipmentVariant> BuildPublishedOffers(CatalogDbContext dbContext)
        => dbContext.EquipmentVariants
            .AsNoTracking()
            .Where(x => x.EquipmentModel.IsPublished && x.EquipmentModel.BasePrice != null);

    public static IQueryable<CatalogOfferRow> SelectRows(IQueryable<EquipmentVariant> query)
        => query.Select(v => new CatalogOfferRow(
                v.Id,
                v.EquipmentModelId,
                v.DisplayName,
                v.EquipmentModel.Brand,
                v.EquipmentModel.Model,
                v.PublicNote,
                v.EquipmentModel.Type,
                v.EquipmentModel.BasePrice!,
                v.OverriddenPrice,
                v.Size));

    public static IOrderedQueryable<EquipmentVariant> OrderByCatalog(IQueryable<EquipmentVariant> query)
        => query
            .OrderBy(x => x.EquipmentModel.Brand)
            .ThenBy(x => x.EquipmentModel.Model)
            .ThenBy(x => x.Size)
            .ThenBy(x => x.Id);

    public static IQueryable<EquipmentVariant> ApplyFilters(IQueryable<EquipmentVariant> query, OfferSearchCriteria criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.Type))
        {
            if (!Enum.TryParse<EquipmentModelType>(criteria.Type, ignoreCase: true, out var type))
                return query.Where(_ => false);

            query = query.Where(x => x.EquipmentModel.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Brand))
            query = query.Where(x => x.EquipmentModel.Brand == criteria.Brand.Trim());

        if (!string.IsNullOrWhiteSpace(criteria.Model))
            query = query.Where(x => x.EquipmentModel.Model == criteria.Model.Trim());

        if (!string.IsNullOrWhiteSpace(criteria.Size))
            query = query.Where(x => x.Size != null && x.Size == criteria.Size.Trim());

        if (criteria.MinPrice is { } min)
        {
            query = query.Where(x =>
                (x.OverriddenPrice != null
                    ? x.OverriddenPrice.Currency.Value
                    : x.EquipmentModel.BasePrice!.Currency.Value) == min.Currency.Value
                &&
                (x.OverriddenPrice != null
                    ? x.OverriddenPrice.Amount
                    : x.EquipmentModel.BasePrice!.Amount) >= min.Amount);
        }

        if (criteria.MaxPrice is { } max)
        {
            query = query.Where(x =>
                (x.OverriddenPrice != null
                    ? x.OverriddenPrice.Currency.Value
                    : x.EquipmentModel.BasePrice!.Currency.Value) == max.Currency.Value
                &&
                (x.OverriddenPrice != null
                    ? x.OverriddenPrice.Amount
                    : x.EquipmentModel.BasePrice!.Amount) <= max.Amount);
        }

        return query;
    }
}

internal sealed record CatalogOfferRow(
    Guid VariantId,
    Guid ModelId,
    string? DisplayName,
    string Brand,
    string Model,
    string? PublicNote,
    EquipmentModelType Type,
    Money BasePrice,
    Money? OverriddenPrice,
    string? Size)
{
    public Money Price => OverriddenPrice ?? BasePrice;
}
