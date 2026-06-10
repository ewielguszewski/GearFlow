using GearFlow.Modules.Catalog.Domain.Entities;
using GearFlow.Modules.Catalog.Domain.Enums;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;

namespace GearFlow.Modules.Catalog.Tests.Unit.Entities;

public class EquipmentModel_Tests
{
    [Fact]
    public void given_no_base_price_publishing_should_fail()
    {
        var unpublishedModel = EquipmentModel.CreateUnpublished("Brand", "Model", EquipmentModelType.Ski);
        unpublishedModel.ChangePublicDetails("Brand", "Model", "Description");

        var exception = Record.Exception(() => unpublishedModel.Publish());
        exception.ShouldNotBeNull();
        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void given_no_description_publishing_should_fail()
    {
        var unpublishedModel = EquipmentModel.CreateUnpublished("Brand", "Model", EquipmentModelType.Ski);
        unpublishedModel.ChangeBasePrice(Money.Create(100, CurrencyCode.PLN));

        var exception = Record.Exception(() => unpublishedModel.Publish());
        exception.ShouldNotBeNull();
        exception.ShouldBeOfType<DomainException>();
    }

    [Fact]
    public void given_all_details_publishing_unpublished_should_succeed()
    {
        var model = EquipmentModel.CreateUnpublished("Brand", "Model", EquipmentModelType.Ski);
        model.ChangePublicDetails("Brand", "Model", "Description");
        model.ChangeBasePrice(Money.Create(100, CurrencyCode.PLN));

        var exception = Record.Exception(() => model.Publish());
        exception.ShouldBeNull();
        model.IsPublished.ShouldBeTrue();
    }
}