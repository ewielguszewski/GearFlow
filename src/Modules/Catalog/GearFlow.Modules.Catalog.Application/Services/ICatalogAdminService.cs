namespace GearFlow.Modules.Catalog.Application.Services;

public interface ICatalogAdminService
{
    Task<Guid> CreateEquipmentModelAsync(CreateEquipmentModelInput input, CancellationToken cancellationToken = default);
    Task<AdminEquipmentModelDto> GetEquipmentModelAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AdminEquipmentModelDto>> GetEquipmentModelsAsync(CancellationToken cancellationToken = default);
    Task<AdminEquipmentItemDetailsDto> GetEquipmentItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task UpdateEquipmentModelAsync(UpdateEquipmentModelInput input, CancellationToken cancellationToken = default);
    Task PublishEquipmentModelAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task UnpublishEquipmentModelAsync(Guid modelId, CancellationToken cancellationToken = default);
    Task<Guid> AddStandardVariantAsync(AddStandardEquipmentVariantInput input, CancellationToken cancellationToken = default);
    Task<Guid> AddPremiumVariantAsync(AddPremiumEquipmentVariantInput input, CancellationToken cancellationToken = default);
    Task<Guid> AddEquipmentItemAsync(AddEquipmentItemInput input, CancellationToken cancellationToken = default);
}
