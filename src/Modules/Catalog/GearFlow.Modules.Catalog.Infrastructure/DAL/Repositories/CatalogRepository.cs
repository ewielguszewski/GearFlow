using GearFlow.Modules.Catalog.Domain.Entities;
using GearFlow.Modules.Catalog.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Catalog.Infrastructure.DAL.Repositories;

public sealed class CatalogRepository : ICatalogRepository
{
    private readonly CatalogDbContext _dbContext;

    public CatalogRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public Task<EquipmentModel?> GetEquipmentModelByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.EquipmentModels
            .Include(em => em.EquipmentVariants)
            .FirstOrDefaultAsync(em => em.Id == id, cancellationToken);

    public Task<EquipmentModel?> GetEquipmentModelByIdForReadAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.EquipmentModels
            .AsNoTracking()
            .Include(em => em.EquipmentVariants)
            .FirstOrDefaultAsync(em => em.Id == id, cancellationToken);

    public Task<EquipmentVariant?> GetEquipmentVariantByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.EquipmentVariants
            .Include(ev => ev.EquipmentModel)
            .FirstOrDefaultAsync(ev => ev.Id == id, cancellationToken);

    public Task<EquipmentItem?> GetEquipmentItemByIdForReadAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.EquipmentItems
            .AsNoTracking()
            .Include(ei => ei.EquipmentVariant)
            .ThenInclude(ev => ev.EquipmentModel)
            .FirstOrDefaultAsync(ei => ei.Id == id, cancellationToken);

    public Task<List<EquipmentModel>> GetAllEquipmentModelsForReadAsync(CancellationToken cancellationToken = default)
        => _dbContext.EquipmentModels
            .AsNoTracking()
            .Include(em => em.EquipmentVariants)
            .ToListAsync(cancellationToken);

    public Task<List<EquipmentVariant>> GetAllEquipmentVariantsAsync(CancellationToken cancellationToken = default)
        => _dbContext.EquipmentVariants
            .AsNoTracking()
            .Include(ev => ev.EquipmentModel)
            .ToListAsync(cancellationToken);

    public  Task<List<EquipmentItem>> GetAllEquipmentItemsAsync(CancellationToken cancellationToken = default)
        => _dbContext.EquipmentItems
            .AsNoTracking()
            .Include(ei => ei.EquipmentVariant)
            .ThenInclude(ev => ev.EquipmentModel)
            .ToListAsync(cancellationToken);

    public Task<List<EquipmentItem>> GetEquipmentItemsByVariantIdsForReadAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken = default)
        => _dbContext.EquipmentItems
            .AsNoTracking()
            .Where(ei => variantIds.Contains(ei.EquipmentVariantId))
            .ToListAsync(cancellationToken);

    public void AddEquipmentModel(EquipmentModel equipmentModel)
        => _dbContext.EquipmentModels.Add(equipmentModel);

    public void UpdateEquipmentModel(EquipmentModel equipmentModel)
        => _dbContext.EquipmentModels.Update(equipmentModel);

    public void AddEquipmentItem(EquipmentItem equipmentItem)
        => _dbContext.EquipmentItems.Add(equipmentItem);

    public void UpdateEquipmentItem(EquipmentItem equipmentItem)
        => _dbContext.EquipmentItems.Update(equipmentItem);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SaveChangesAsync(cancellationToken);

}
