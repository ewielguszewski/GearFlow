using GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;
using GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;
using GearFlow.Modules.Reservations.Application.Queries.GetAvailableOffers;
using GearFlow.Modules.Reservations.Application.Queries.GetReservationDraft;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GearFlow.Api.Controllers.Reservations;

[ApiController]
[Route("api/reservations")]
public class ReservationsDraftController : ControllerBase
{
    private const string GetReservationDraftRouteName = "GetReservationDraft";

    private readonly ICommandHandler<CreateDraftReservationCommand> _createDraftHandler;
    private readonly ICommandHandler<AddReservationLineCommand> _addLineHandler;
    private readonly IQueryHandler<GetAvailableOffers, IEnumerable<AvailableOfferDto>> _getAvailableOffersHandler;
    private readonly IQueryHandler<GetReservationDraft, ReservationDraftDto?> _getReservationDraftHandler;

    public ReservationsDraftController(ICommandHandler<CreateDraftReservationCommand> createDraftHandler, ICommandHandler<AddReservationLineCommand> addLineHandler,
        IQueryHandler<GetAvailableOffers, IEnumerable<AvailableOfferDto>> getAvailableOffersHandler, IQueryHandler<GetReservationDraft, ReservationDraftDto?> getReservationDraftHandler)
    {
        _createDraftHandler = createDraftHandler;
        _addLineHandler = addLineHandler;
        _getAvailableOffersHandler = getAvailableOffersHandler;
        _getReservationDraftHandler = getReservationDraftHandler;
    }

    [HttpPost("drafts")]
    public async Task<ActionResult> CreateDraftAsync([FromBody] CreateDraftReservationRequest request, CancellationToken cancellationToken)
    {
        var reservationId = Guid.NewGuid();

        var command = new CreateDraftReservationCommand(reservationId, request.CustomerId, request.From, request.To, request.Currency ?? "PLN");

        await _createDraftHandler.HandleAsync(command, cancellationToken);

        return CreatedAtRoute(GetReservationDraftRouteName, new { draftId = reservationId }, new { reservationId });
    }

    [HttpGet("drafts/{draftId:guid}", Name = GetReservationDraftRouteName)]
    public async Task<ActionResult<ReservationDraftDto>> GetReservationDraftAsync([FromRoute] Guid draftId, CancellationToken cancellationToken)
    {
        // for now can take id from route, but in future we can take it from user context as can only have one draft per user

        var draft = await _getReservationDraftHandler.HandleAsync(new GetReservationDraft(draftId), cancellationToken);

        return Ok(draft);
    }

    [HttpGet("drafts/{draftId:guid}/offers")]
    public async Task<ActionResult<IReadOnlyCollection<AvailableOfferResponse>>> GetAvailableOffersAsync([FromRoute] Guid draftId, [FromQuery] GetAvailableOffersRequest request, CancellationToken cancellationToken)
    {
        var currency = CurrencyCode.From(request.Currency ?? "PLN");

        var query = new GetAvailableOffers(
            draftId,
            request.Type,
            request.Brand,
            request.Model,
            CreateMoney(request.MinPrice, currency),
            CreateMoney(request.MaxPrice, currency), 
            request.Size,
            request.Page,
            request.PageSize
            );

        var offers = await _getAvailableOffersHandler.HandleAsync(query, cancellationToken);

        var offersDto = offers.Select(o => new AvailableOfferResponse(
            o.VariantId,
            o.Brand,
            o.Model,
            o.Type,
            o.PricePerDay.Amount,
            o.PricePerDay.Currency.Value,
            o.Size,
            o.AvailableCount
        )).ToList();

        return Ok(offersDto);
    }

    [HttpPost("drafts/{draftId:guid}/lines")]
    public async Task<ActionResult> AddLineAsync([FromRoute] Guid draftId, [FromBody] AddReservationLineRequest request, CancellationToken cancellationToken)
    {
        var reservationLineId = Guid.NewGuid();

        var command = new AddReservationLineCommand(draftId, reservationLineId, request.OfferVariantId);

        await _addLineHandler.HandleAsync(command, cancellationToken);

        return CreatedAtRoute(GetReservationDraftRouteName, new { draftId }, new { reservationLineId });
    }

    private static Money? CreateMoney(decimal? amount, CurrencyCode currency)
        => amount.HasValue ? Money.Create(amount.Value, currency) : null;
}

public sealed class CreateDraftReservationRequest
{
    public Guid CustomerId { get; init; }
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public string? Currency { get; init; } = "PLN";
}

public sealed record GetAvailableOffersRequest
{
    public string? Type { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Currency { get; init; } = "PLN";
    public string? Size { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

public sealed record AddReservationLineRequest
{
    public Guid OfferVariantId { get; init; }
}

public sealed record AvailableOfferResponse(
    Guid VariantId,
    string Brand,
    string Model,
    string Type,
    decimal PricePerDay,
    string Currency,
    string? Size,
    int AvailableCount
    );
