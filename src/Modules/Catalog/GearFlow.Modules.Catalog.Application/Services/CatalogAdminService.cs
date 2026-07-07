using GearFlow.Modules.Catalog.Domain.Entities;
using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Modules.Catalog.Domain.Repositories;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Catalog.Application.Services;

public sealed class CatalogAdminService : ICatalogAdminService
{
    private readonly ICatalogRepository _catalogRepository;

    public CatalogAdminService(ICatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    public async Task<Guid> CreateEquipmentModelAsync(CreateEquipmentModelInput input, CancellationToken cancellationToken = default)
    {
        var type = ParseEquipmentModelType(input.Type);
        var currency = CurrencyCode.From(input.Currency);

        var model = input.Publish
            ? EquipmentModel.CreatePublished(
                input.Brand,
                input.Model,
                RequireValue(input.Description, "Description is required to publish an equipment model."),
                Money.Create(RequireValue(input.BasePrice, "Base price is required to publish an equipment model."), currency),
                type)
            : EquipmentModel.CreateUnpublished(input.Brand, input.Model, type);

        if (!input.Publish)
        {
            if (!string.IsNullOrWhiteSpace(input.Description))
                model.ChangePublicDetails(input.Brand, input.Model, input.Description);

            if (input.BasePrice.HasValue)
                model.ChangeBasePrice(Money.Create(input.BasePrice.Value, currency));
        }

        _catalogRepository.AddEquipmentModel(model);
        await _catalogRepository.SaveChangesAsync(cancellationToken);

        return model.Id;
    }

    public async Task<AdminEquipmentModelDto> GetEquipmentModelAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        var model = await GetModelForReadOrThrowAsync(modelId, cancellationToken);
        var items = await GetItemsByVariantForReadAsync(model, cancellationToken);

        return MapAdminModel(model, items);
    }

    public async Task<IReadOnlyCollection<AdminEquipmentModelDto>> GetEquipmentModelsAsync(CancellationToken cancellationToken = default)
    {
        var models = await _catalogRepository.GetAllEquipmentModelsForReadAsync(cancellationToken);
        var variantIds = models
            .SelectMany(x => x.EquipmentVariants)
            .Select(x => x.Id)
            .ToArray();

        var items = await _catalogRepository.GetEquipmentItemsByVariantIdsForReadAsync(variantIds, cancellationToken);
        var itemsByVariant = items
            .GroupBy(x => x.EquipmentVariantId)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<EquipmentItem>)x.ToArray());

        return models
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Model)
            .Select(x => MapAdminModel(x, itemsByVariant))
            .ToArray();
    }

    public async Task<AdminEquipmentItemDetailsDto> GetEquipmentItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _catalogRepository.GetEquipmentItemByIdForReadAsync(itemId, cancellationToken);
        if (item is null)
            throw new NotFoundException("Equipment item not found.");

        return new AdminEquipmentItemDetailsDto(
            item.Id,
            item.EquipmentVariantId,
            item.EquipmentVariant.EquipmentModelId,
            item.EquipmentVariant.EquipmentModel.Brand,
            item.EquipmentVariant.EquipmentModel.Model,
            item.EquipmentVariant.Size,
            item.AssetTag,
            item.InternalNote,
            item.Status.ToString(),
            item.LastMaintenanceAt,
            item.NextInspectionAt,
            item.CreatedAt);
    }

    public async Task UpdateEquipmentModelAsync(UpdateEquipmentModelInput input, CancellationToken cancellationToken = default)
    {
        var model = await GetModelOrThrowAsync(input.ModelId, cancellationToken);

        model.ChangePublicDetails(input.Brand, input.Model, input.Description);

        if (input.BasePrice.HasValue)
            model.ChangeBasePrice(Money.Create(input.BasePrice.Value, CurrencyCode.From(input.Currency)));

        await _catalogRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishEquipmentModelAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        var model = await GetModelOrThrowAsync(modelId, cancellationToken);

        model.Publish();
        await _catalogRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishEquipmentModelAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        var model = await GetModelOrThrowAsync(modelId, cancellationToken);

        model.Unpublish();
        await _catalogRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> AddStandardVariantAsync(AddStandardEquipmentVariantInput input, CancellationToken cancellationToken = default)
    {
        var model = await GetModelOrThrowAsync(input.ModelId, cancellationToken);
        var variant = model.AddStandardVariant(RequireValue(input.Size, "Size is required."));

        await _catalogRepository.SaveChangesAsync(cancellationToken);

        return variant.Id;
    }

    public async Task<Guid> AddPremiumVariantAsync(AddPremiumEquipmentVariantInput input, CancellationToken cancellationToken = default)
    {
        var model = await GetModelOrThrowAsync(input.ModelId, cancellationToken);
        var variant = model.AddPremiumVariant(
            input.DisplayName,
            input.PublicNote,
            Money.Create(input.PriceOverride, CurrencyCode.From(input.Currency)),
            input.Size);

        await _catalogRepository.SaveChangesAsync(cancellationToken);

        return variant.Id;
    }

    public async Task<Guid> AddEquipmentItemAsync(AddEquipmentItemInput input, CancellationToken cancellationToken = default)
    {
        var variant = await _catalogRepository.GetEquipmentVariantByIdAsync(input.VariantId, cancellationToken);
        if (variant == null)
            throw new NotFoundException("Equipment variant not found.");

        var item = EquipmentItem.Create(
            variant.Id,
            input.AssetTag,
            input.InternalNote,
            ParseEquipmentItemStatus(input.Status));

        _catalogRepository.AddEquipmentItem(item);
        await _catalogRepository.SaveChangesAsync(cancellationToken);

        return item.Id;
    }

    private async Task<EquipmentModel> GetModelOrThrowAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var model = await _catalogRepository.GetEquipmentModelByIdAsync(modelId, cancellationToken);

        return model ?? throw new NotFoundException("Equipment model not found.");
    }

    private async Task<EquipmentModel> GetModelForReadOrThrowAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var model = await _catalogRepository.GetEquipmentModelByIdForReadAsync(modelId, cancellationToken);

        return model ?? throw new NotFoundException("Equipment model not found.");
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EquipmentItem>>> GetItemsByVariantForReadAsync(
        EquipmentModel model,
        CancellationToken cancellationToken)
    {
        var variantIds = model.EquipmentVariants.Select(x => x.Id).ToArray();
        var items = await _catalogRepository.GetEquipmentItemsByVariantIdsForReadAsync(variantIds, cancellationToken);

        return items
            .GroupBy(x => x.EquipmentVariantId)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<EquipmentItem>)x.ToArray());
    }

    private static AdminEquipmentModelDto MapAdminModel(
        EquipmentModel model,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<EquipmentItem>> itemsByVariant)
        => new(
            model.Id,
            model.Brand,
            model.Model,
            model.Slug,
            model.Description,
            model.IsPublished,
            model.Type.ToString(),
            model.BasePrice?.Amount,
            model.BasePrice?.Currency.Value,
            model.EquipmentVariants
                .OrderBy(x => x.Size)
                .ThenBy(x => x.DisplayName)
                .Select(x => MapAdminVariant(x, itemsByVariant))
                .ToArray());

    private static AdminEquipmentVariantDto MapAdminVariant(
        EquipmentVariant variant,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<EquipmentItem>> itemsByVariant)
        => new(
            variant.Id,
            variant.DisplayName,
            variant.PublicNote,
            variant.Size,
            variant.OverriddenPrice?.Amount,
            variant.OverriddenPrice?.Currency.Value,
            itemsByVariant.TryGetValue(variant.Id, out var items)
                ? items.OrderBy(x => x.AssetTag).Select(MapAdminItem).ToArray()
                : []);

    private static AdminEquipmentItemDto MapAdminItem(EquipmentItem item)
        => new(
            item.Id,
            item.AssetTag,
            item.InternalNote,
            item.Status.ToString(),
            item.LastMaintenanceAt,
            item.NextInspectionAt,
            item.CreatedAt);

    private static EquipmentModelType ParseEquipmentModelType(string value)
    {
        if (Enum.TryParse<EquipmentModelType>(value, ignoreCase: true, out var type))
            return type;

        throw new DomainException($"Unsupported equipment model type '{value}'.");
    }

    private static EquipmentItemStatus ParseEquipmentItemStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return EquipmentItemStatus.Active;

        if (Enum.TryParse<EquipmentItemStatus>(value, ignoreCase: true, out var status))
            return status;

        throw new DomainException($"Unsupported equipment item status '{value}'.");
    }

    private static T RequireValue<T>(T? value, string message)
        where T : struct
        => value ?? throw new DomainException(message);

    private static string RequireValue(string? value, string message)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        throw new DomainException(message);
    }
}
