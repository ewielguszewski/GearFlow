using GearFlow.Modules.Reservations.Application.Commands.RemoveReservationLine;
using GearFlow.Modules.Reservations.Domain.Entities;
using Shouldly;
using static GearFlow.Reservations.Tests.Unit.Application.ReservationApplicationTestData;

namespace GearFlow.Reservations.Tests.Unit.Application;

public class RemoveReservationLineHandler_Tests
{
    [Fact]
    public async Task remove_line_should_release_held_item_allocation()
    {
        var itemId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var reservation = CreateDraft();
        reservation.AddReservationLine(lineId, CreateOfferSnapshot(itemId), Now);
        var availability = new FakeAvailabilityAllocator();
        var handler = CreateHandler(reservation, availability);

        await handler.HandleAsync(new RemoveReservationLineCommand(reservation.Id, lineId), CancellationToken.None);

        reservation.ReservationLines.ShouldBeEmpty();
        availability.ReleasedSourceId.ShouldBe(reservation.Id);
        availability.ReleasedItemId.ShouldBe(itemId);
    }

    [Fact]
    public async Task missing_line_should_be_noop()
    {
        var reservation = CreateDraft();
        var availability = new FakeAvailabilityAllocator();
        var handler = CreateHandler(reservation, availability);

        await handler.HandleAsync(new RemoveReservationLineCommand(reservation.Id, Guid.NewGuid()), CancellationToken.None);

        availability.ReleasedSourceId.ShouldBeNull();
        availability.ReleasedItemId.ShouldBeNull();
    }

    private static RemoveReservationLineHandler CreateHandler(Reservation reservation, FakeAvailabilityAllocator availability)
        => new(
            new FakeReservationRepository(reservation),
            availability,
            new FakeReservationAuthorizationService(),
            new FixedClock(Now));
}
