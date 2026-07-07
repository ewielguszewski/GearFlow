using GearFlow.Modules.Catalog.Domain.Entities;

namespace GearFlow.Modules.Catalog.Domain.Repositories;

public interface ICatalogRepository
{
    Task<EquipmentModel?> GetEquipmentModelByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EquipmentModel?> GetEquipmentModelByIdForReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EquipmentVariant?> GetEquipmentVariantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EquipmentItem?> GetEquipmentItemByIdForReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<EquipmentModel>> GetAllEquipmentModelsForReadAsync(CancellationToken cancellationToken = default);
    Task<List<EquipmentVariant>> GetAllEquipmentVariantsAsync(CancellationToken cancellationToken = default);
    Task<List<EquipmentItem>> GetAllEquipmentItemsAsync(CancellationToken cancellationToken = default);
    Task<List<EquipmentItem>> GetEquipmentItemsByVariantIdsForReadAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken = default);
    void AddEquipmentModel(EquipmentModel equipmentModel);
    void UpdateEquipmentModel(EquipmentModel equipmentModel);
    void AddEquipmentItem(EquipmentItem equipmentItem);
    void UpdateEquipmentItem(EquipmentItem equipmentItem);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
