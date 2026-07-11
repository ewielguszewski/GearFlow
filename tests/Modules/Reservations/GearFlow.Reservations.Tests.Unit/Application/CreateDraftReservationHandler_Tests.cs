using GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;
using GearFlow.Modules.Reservations.Domain.Entities;
using Shouldly;
using static GearFlow.Reservations.Tests.Unit.Application.ReservationApplicationTestData;

namespace GearFlow.Reservations.Tests.Unit.Application;

public class CreateDraftReservationHandler_Tests
{
    [Fact]
    public async Task existing_draft_with_lines_should_be_replaced_and_release_allocations()
    {
        var customerId = Guid.NewGuid();
        var existingDraft = CreateDraft(customerId);
        existingDraft.AddReservationLine(Guid.NewGuid(), CreateOfferSnapshot(), Now);
        var repository = new FakeReservationRepository(existingDraft);
        var availability = new FakeAvailabilityAllocator();
        var handler = new CreateDraftReservationHandler(
            repository,
            availability,
            new FakeReservationAuthorizationService(),
            new FixedClock(Now));
        var newReservationId = Guid.NewGuid();

        await handler.HandleAsync(new CreateDraftReservationCommand(
            newReservationId,
            customerId,
            Now.Date.AddDays(3),
            Now.Date.AddDays(5),
            "PLN"));

        availability.ReleasedSourceId.ShouldBe(existingDraft.Id);
        existingDraft.Status.ShouldBe(ReservationStatus.Cancelled);
        existingDraft.CancReason.ShouldBe(CancellationReason.ReplacedByNewDraft);
        repository.AddedReservation.ShouldNotBeNull();
        repository.AddedReservation.Id.ShouldBe(newReservationId);
    }

    [Fact]
    public async Task new_draft_should_use_customer_id_resolved_by_authorization()
    {
        var customerId = Guid.NewGuid();
        var repository = new FakeReservationRepository();
        var handler = new CreateDraftReservationHandler(
            repository,
            new FakeAvailabilityAllocator(),
            new FakeReservationAuthorizationService { ResolvedCustomerId = customerId },
            new FixedClock(Now));

        await handler.HandleAsync(new CreateDraftReservationCommand(
            Guid.NewGuid(),
            null,
            Now.Date.AddDays(1),
            Now.Date.AddDays(2),
            "PLN"));

        repository.AddedReservation.ShouldNotBeNull();
        repository.AddedReservation.CustomerId.ShouldBe(customerId);
    }
}
