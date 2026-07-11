using GearFlow.Modules.Availability.Core.Entities;
using GearFlow.Modules.Availability.Infrastructure.DAL;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.ValueObjects;
using GearFlow.Modules.Reservations.Infrastructure.Background;
using GearFlow.Modules.Reservations.Infrastructure.DAL;
using GearFlow.Modules.Users.Core.Auth.DTO;
using GearFlow.Shared.Abstractions.ValueObjects;
using GearFlow.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GearFlow.Tests.Integration.Reservations;

public class ReservationFlow_Tests : IClassFixture<GearFlowIntegrationFixture>
{
    private readonly GearFlowIntegrationFixture _fixture;

    public ReservationFlow_Tests(GearFlowIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task reservation_draft_flow_should_hold_release_and_confirm_item()
    {
        var customer = await SignUpCustomerAsync();
        var draftId = await CreateDraftAsync(customer.Client);
        var offer = await GetFirstAvailableOfferAsync(customer.Client, draftId);

        var lineId = await AddLineAsync(customer.Client, draftId, offer.VariantId);

        var draftWithLine = await GetDraftAsync(customer.Client, draftId);
        Assert.Equal("Draft", draftWithLine.Status);
        Assert.Single(draftWithLine.ReservedItems);
        Assert.Equal(lineId, draftWithLine.ReservedItems.Single().ReservationLineId);
        Assert.Equal(1, await CountBookingsAsync(draftId));

        var removeResponse = await customer.Client.DeleteAsync($"/api/reservations/drafts/{draftId}/lines/{lineId}");
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        var draftAfterRemove = await GetDraftAsync(customer.Client, draftId);
        Assert.Empty(draftAfterRemove.ReservedItems);
        Assert.Equal(0, await CountBookingsAsync(draftId));

        await AddLineAsync(customer.Client, draftId, offer.VariantId);

        var confirmResponse = await customer.Client.PostAsJsonAsync(
            $"/api/reservations/drafts/{draftId}/confirm",
            new { paymentMethod = "CashOnPickup" });
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var confirmedDraft = await GetDraftAsync(customer.Client, draftId);
        Assert.Equal("Confirmed", confirmedDraft.Status);
        Assert.Equal(1, await CountBookingsAsync(draftId));
    }

    [Fact]
    public async Task creating_second_draft_for_customer_should_leave_only_one_active_draft()
    {
        var customer = await SignUpCustomerAsync();

        await CreateDraftAsync(customer.Client);
        var secondDraftId = await CreateDraftAsync(customer.Client);

        using var scope = _fixture.ApiFactory.Services.CreateScope();
        var reservations = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        var drafts = await reservations.Reservations
            .Where(x => x.CustomerId == customer.CustomerId)
            .ToArrayAsync();

        Assert.Equal(2, drafts.Length);
        Assert.Single(drafts.Where(x => x.Status == ReservationStatus.Draft));
        Assert.Contains(drafts, x => x.Id == secondDraftId && x.Status == ReservationStatus.Draft);
        Assert.Contains(drafts, x => x.Status == ReservationStatus.Cancelled && x.CancReason == CancellationReason.ReplacedByNewDraft);
    }

    [Fact]
    public async Task item_booking_overlap_constraint_should_reject_same_item_in_inclusive_period()
    {
        using var scope = _fixture.ApiFactory.Services.CreateScope();
        var availability = scope.ServiceProvider.GetRequiredService<AvailabilityDbContext>();
        var itemId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var start = DateTime.UtcNow.Date.AddDays(1);

        availability.Bookings.Add(ItemBooking.Create(
            itemId,
            variantId,
            new DateRange(start, start.AddDays(2)),
            Guid.NewGuid(),
            BookingType.Reservation));
        await availability.SaveChangesAsync();

        availability.Bookings.Add(ItemBooking.Create(
            itemId,
            variantId,
            new DateRange(start.AddDays(2), start.AddDays(4)),
            Guid.NewGuid(),
            BookingType.Reservation));

        await Assert.ThrowsAsync<DbUpdateException>(() => availability.SaveChangesAsync());
    }

    [Fact]
    public async Task expired_draft_processor_should_cancel_expired_drafts_and_release_reservation_bookings()
    {
        using var scope = _fixture.ApiFactory.Services.CreateScope();
        var reservations = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        var availability = scope.ServiceProvider.GetRequiredService<AvailabilityDbContext>();
        var processor = scope.ServiceProvider.GetRequiredService<IExpiredDraftReservationProcessor>();

        var expiredDraft = CreateDraftWithLine(DateTime.UtcNow.AddMinutes(-10));
        var activeDraft = CreateDraftWithLine(DateTime.UtcNow);
        reservations.Reservations.AddRange(expiredDraft.Reservation, activeDraft.Reservation);
        availability.Bookings.AddRange(expiredDraft.Booking, activeDraft.Booking);
        await reservations.SaveChangesAsync();
        await availability.SaveChangesAsync();

        var processedCount = await processor.ProcessExpiredDraftsAsync();

        Assert.Equal(1, processedCount);
        Assert.Equal(ReservationStatus.Cancelled, expiredDraft.Reservation.Status);
        Assert.Equal(CancellationReason.DraftExpired, expiredDraft.Reservation.CancReason);
        Assert.Equal(ReservationStatus.Draft, activeDraft.Reservation.Status);
        Assert.Equal(0, await CountBookingsAsync(expiredDraft.Reservation.Id));
        Assert.Equal(1, await CountBookingsAsync(activeDraft.Reservation.Id));
    }

    private HttpClient CreateClient()
        => _fixture.ApiFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    private async Task<AuthenticatedCustomer> SignUpCustomerAsync()
    {
        var client = CreateClient();
        var unique = Guid.NewGuid().ToString("N")[..12];

        var request = new SignUpRequest
        {
            Email = $"c-{unique}@gf.test",
            Password = "P@ssword123!",
            FirstName = "Test",
            LastName = "Customer",
            PhoneNumber = "+48123123123"
        };

        var signUpResponse = await client.PostAsJsonAsync("/api/auth/sign-up", request);
        Assert.Equal(HttpStatusCode.OK, signUpResponse.StatusCode);

        var auth = await signUpResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var me = await client.GetFromJsonAsync<MeResponse>("/api/auth/me");
        Assert.NotNull(me);
        Assert.NotNull(me.CustomerId);

        return new AuthenticatedCustomer(client, me.CustomerId.Value);
    }

    private static async Task<Guid> CreateDraftAsync(HttpClient client)
    {
        var start = DateTime.UtcNow.Date.AddDays(1);
        var response = await client.PostAsJsonAsync("/api/reservations/drafts", new
        {
            from = start,
            to = start.AddDays(2),
            currency = "PLN"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateDraftResponse>();
        Assert.NotNull(body);
        return body.ReservationId;
    }

    private static async Task<AvailableOfferResponse> GetFirstAvailableOfferAsync(HttpClient client, Guid draftId)
    {
        var offers = await client.GetFromJsonAsync<IReadOnlyCollection<AvailableOfferResponse>>(
            $"/api/reservations/drafts/{draftId}/offers");

        Assert.NotNull(offers);
        Assert.NotEmpty(offers);

        return offers.First(x => x.AvailableCount > 0);
    }

    private static async Task<Guid> AddLineAsync(HttpClient client, Guid draftId, Guid offerVariantId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/reservations/drafts/{draftId}/lines",
            new { offerVariantId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AddLineResponse>();
        Assert.NotNull(body);
        return body.ReservationLineId;
    }

    private static async Task<ReservationDraftResponse> GetDraftAsync(HttpClient client, Guid draftId)
    {
        var draft = await client.GetFromJsonAsync<ReservationDraftResponse>(
            $"/api/reservations/drafts/{draftId}");

        Assert.NotNull(draft);
        return draft;
    }

    private async Task<int> CountBookingsAsync(Guid sourceId)
    {
        using var scope = _fixture.ApiFactory.Services.CreateScope();
        var availability = scope.ServiceProvider.GetRequiredService<AvailabilityDbContext>();

        return await availability.Bookings.CountAsync(x => x.Source == BookingType.Reservation && x.SourceId == sourceId);
    }

    private static DraftWithBooking CreateDraftWithLine(DateTime createdAt)
    {
        var itemId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var period = new DateRange(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3));
        var reservation = Reservation.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            period,
            CurrencyCode.PLN,
            createdAt);

        reservation.AddReservationLine(
            Guid.NewGuid(),
            OfferSnapshot.Create(
                itemId,
                variantId,
                "Default",
                "Brand",
                "Model",
                null,
                Money.CreateFromPln(100),
                PriceSource.CatalogModel,
                "M"),
            createdAt);

        var booking = ItemBooking.Create(
            itemId,
            variantId,
            period,
            reservation.Id,
            BookingType.Reservation);

        return new DraftWithBooking(reservation, booking);
    }

    private sealed record CreateDraftResponse(Guid ReservationId);

    private sealed record AddLineResponse(Guid ReservationLineId);

    private sealed record AuthResponse(string AccessToken, string RefreshToken);

    private sealed record MeResponse(Guid UserId, Guid? CustomerId, string Role);

    private sealed record AuthenticatedCustomer(HttpClient Client, Guid CustomerId);

    private sealed record AvailableOfferResponse(
        Guid VariantId,
        string Brand,
        string Model,
        string Type,
        decimal PricePerDay,
        string Currency,
        string? Size,
        int AvailableCount);

    private sealed record ReservationDraftResponse(
        Guid DraftId,
        Guid CustomerId,
        string Status,
        DateTime StartDate,
        DateTime EndDate,
        DateTime TtlExpiresAt,
        bool IsExpired,
        string Currency,
        decimal TotalPrice,
        IReadOnlyCollection<ReservedItemResponse> ReservedItems);

    private sealed record ReservedItemResponse(
        Guid ReservationLineId,
        Guid VariantId,
        string Model,
        string Brand,
        decimal BasePrice,
        decimal LineTotalPrice);

    private sealed record DraftWithBooking(Reservation Reservation, ItemBooking Booking);
}
