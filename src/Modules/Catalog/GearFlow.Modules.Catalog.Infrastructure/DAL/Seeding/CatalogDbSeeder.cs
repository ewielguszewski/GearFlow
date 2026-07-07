using GearFlow.Modules.Catalog.Domain.Entities;
using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Catalog.Infrastructure.DAL.Seeding;

public sealed class CatalogDbSeeder
{
    private readonly CatalogDbContext _dbContext;
    public CatalogDbSeeder(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.EquipmentModels.AnyAsync(cancellationToken))
            return;

        // EquipmentModels and EquipmentVariants
        var skis = EquipmentModel.CreatePublished("Fischer", "RC4 Worldcup", "Performance alpine skis for competitive skiing.", Money.CreateFromPln(50), EquipmentModelType.Ski);
        var ski1 = skis.AddStandardVariant("170cm");
        var ski2 = skis.AddStandardVariant("180cm");

        var boots = EquipmentModel.CreatePublished("Salomon", "X Pro 100", "High-performance alpine ski boots for advanced skiers.", Money.CreateFromPln(30), EquipmentModelType.Boots);
        var boot1 = boots.AddStandardVariant("26.5");
        var boot2 = boots.AddStandardVariant("27.5");

        var unpublished = EquipmentModel.CreateUnpublished("Atomic", "Redster X9", EquipmentModelType.Ski);
        var unpublishedVariant = unpublished.AddStandardVariant("175cm");

        await _dbContext.EquipmentModels.AddRangeAsync([skis, boots, unpublished], cancellationToken);
        await _dbContext.EquipmentVariants.AddRangeAsync([ski1, ski2, boot1, boot2, unpublishedVariant], cancellationToken);

        // EquipmentItems
        var skiItem1 = EquipmentItem.Create(ski1.Id, "SN123456", null, EquipmentItemStatus.Active);
        var skiItem2 = EquipmentItem.Create(ski1.Id, "SN123457", null, EquipmentItemStatus.Broken);

        var bootItem1 = EquipmentItem.Create(boot1.Id, "BT123456", null, EquipmentItemStatus.Active);
        var bootItem2 = EquipmentItem.Create(boot1.Id, "BT123457", null, EquipmentItemStatus.Active);

        var unpublishedItem = EquipmentItem.Create(unpublishedVariant.Id, "UN123456", null, EquipmentItemStatus.Active);

        await _dbContext.EquipmentItems.AddRangeAsync([skiItem1, skiItem2, bootItem1, bootItem2, unpublishedItem], cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}