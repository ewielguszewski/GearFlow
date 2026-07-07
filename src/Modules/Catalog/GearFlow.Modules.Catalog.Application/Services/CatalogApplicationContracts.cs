namespace GearFlow.Modules.Catalog.Application.Services;

public sealed record CreateEquipmentModelInput(
    string Brand,
    string Model,
    string Type,
    string? Description,
    decimal? BasePrice,
    string Currency,
    bool Publish);

public sealed record UpdateEquipmentModelInput(
    Guid ModelId,
    string Brand,
    string Model,
    string Description,
    decimal? BasePrice,
    string Currency);

public sealed record AddStandardEquipmentVariantInput(Guid ModelId, string Size);

public sealed record AddPremiumEquipmentVariantInput(
    Guid ModelId,
    string DisplayName,
    string PublicNote,
    decimal PriceOverride,
    string Currency,
    string? Size);

public sealed record AddEquipmentItemInput(
    Guid VariantId,
    string AssetTag,
    string? InternalNote,
    string? Status);

public sealed record BrowseCatalogOffersInput(
    string? Type,
    string? Brand,
    string? Model,
    string? Size,
    decimal? MinPrice,
    decimal? MaxPrice,
    string Currency,
    int Page,
    int PageSize);

public sealed record AdminEquipmentModelDto(
    Guid Id,
    string Brand,
    string Model,
    string Slug,
    string? Description,
    bool IsPublished,
    string Type,
    decimal? BasePrice,
    string? Currency,
    IReadOnlyCollection<AdminEquipmentVariantDto> Variants);

public sealed record AdminEquipmentVariantDto(
    Guid Id,
    string? DisplayName,
    string? PublicNote,
    string? Size,
    decimal? OverriddenPrice,
    string? Currency,
    IReadOnlyCollection<AdminEquipmentItemDto> Items);

public sealed record AdminEquipmentItemDto(
    Guid Id,
    string AssetTag,
    string? InternalNote,
    string Status,
    DateTime? LastMaintenanceAt,
    DateTime NextInspectionAt,
    DateTime CreatedAt);

public sealed record AdminEquipmentItemDetailsDto(
    Guid Id,
    Guid VariantId,
    Guid ModelId,
    string Brand,
    string Model,
    string? Size,
    string AssetTag,
    string? InternalNote,
    string Status,
    DateTime? LastMaintenanceAt,
    DateTime NextInspectionAt,
    DateTime CreatedAt);

public sealed record PublicCatalogOfferDto(
    Guid VariantId,
    Guid ModelId,
    string Brand,
    string Model,
    string Type,
    string? Size,
    string? VariantName,
    string? PublicNote,
    decimal PricePerDay,
    string Currency,
    bool IsIndividuallyPriced);

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
