using GearFlow.Modules.Catalog.Domain.Entities;
using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;

namespace GearFlow.Modules.Catalog.Tests.Unit.Entities;

public class EquipmentItem_Tests
{

    [Theory]
    [InlineData(EquipmentItemStatus.Unavailable)]
    [InlineData(EquipmentItemStatus.RequiresInspection)]
    [InlineData(EquipmentItemStatus.Broken)]
    [InlineData(EquipmentItemStatus.Lost)]
    [InlineData(EquipmentItemStatus.Retired)]
    public void given_non_rentable_status_ensure_rentable_should_fail(EquipmentItemStatus status)
    {
        var item = EquipmentItem.Create(Guid.NewGuid(), "Code-123", null, status);

        var exception = Record.Exception(() => item.EnsureRentable());

        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void given_active_status_ensure_rentable_should_succeed()
    {
        var item = EquipmentItem.Create(Guid.NewGuid(), "Code-123", null, EquipmentItemStatus.Active);

        var exception = Record.Exception(() => item.EnsureRentable());

        exception.ShouldBeNull();
    }
}
