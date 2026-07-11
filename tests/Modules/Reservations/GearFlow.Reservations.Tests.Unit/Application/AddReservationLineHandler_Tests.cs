using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.ValueObjects;
using Shouldly;
using static GearFlow.Reservations.Tests.Unit.Application.ReservationApplicationTestData;

namespace GearFlow.Reservations.Tests.Unit.Application;

public class AddReservationLineHandler_Tests
{
    [Fact]
    public async Task expired_draft_should_fail_without_cleanup_side_effects()
    {
        var reservation = CreateDraft();
        var availability = new FakeAvailabilityAllocator();
        var handler = CreateHandler(
            reservation,
            availability: availability,
            now: reservation.TtlExpiresAt.AddMinutes(1));

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(new AddReservationLineCommand(
            reservation.Id,
            Guid.NewGuid(),
            Guid.NewGuid())));

        exception.ShouldBeOfType<DomainException>();
        availability.ReleasedSourceId.ShouldBeNull();
        reservation.Status.ShouldBe(ReservationStatus.Draft);
        reservation.CancReason.ShouldBeNull();
    }

    [Fact]
    public async Task add_line_should_pass_variant_id_to_availability_allocator()
    {
        var reservation = CreateDraft();
        var variantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var availability = new FakeAvailabilityAllocator
        {
            ItemToAllocate = itemId
        };
        var catalog = new FakeCatalogOfferReader
        {
            Offer = new ReservableOfferDto(
                variantId,
                "Premium",
                "Brand",
                "Model",
                "Public note",
                Money.CreateFromPln(100),
                null,
                "M",
                [itemId])
        };
        var handler = CreateHandler(reservation, catalog, availability);
        var reservationLineId = Guid.NewGuid();

        await handler.HandleAsync(new AddReservationLineCommand(
            reservation.Id,
            reservationLineId,
            variantId));

        availability.AllocatedVariantId.ShouldBe(variantId);
        availability.AllocatedSourceId.ShouldBe(reservation.Id);
        reservation.ReservationLines.Single().Id.ShouldBe(reservationLineId);
    }

    [Fact]
    public async Task authorization_failure_should_stop_before_allocation()
    {
        var reservation = CreateDraft();
        var availability = new FakeAvailabilityAllocator();
        var authorization = new FakeReservationAuthorizationService
        {
            ExceptionToThrow = new ForbiddenException()
        };
        var handler = CreateHandler(reservation, availability: availability, authorization: authorization);

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(new AddReservationLineCommand(
            reservation.Id,
            Guid.NewGuid(),
            Guid.NewGuid())));

        exception.ShouldBeOfType<ForbiddenException>();
        availability.AllocatedSourceId.ShouldBeNull();
        reservation.ReservationLines.ShouldBeEmpty();
    }

    private static AddReservationLineHandler CreateHandler(
        Reservation reservation,
        FakeCatalogOfferReader? catalog = null,
        FakeAvailabilityAllocator? availability = null,
        FakeReservationAuthorizationService? authorization = null,
        DateTime? now = null)
        => new(
            new FakeReservationRepository(reservation),
            catalog ?? new FakeCatalogOfferReader(),
            availability ?? new FakeAvailabilityAllocator(),
            authorization ?? new FakeReservationAuthorizationService(),
            new FixedClock(now ?? reservation.CreatedAt));
}
