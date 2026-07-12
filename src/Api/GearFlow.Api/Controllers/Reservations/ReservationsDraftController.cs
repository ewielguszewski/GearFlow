using GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;
using GearFlow.Modules.Reservations.Application.Commands.ConfirmReservationDraft;
using GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;
using GearFlow.Modules.Reservations.Application.Commands.RemoveReservationLine;
using GearFlow.Modules.Reservations.Application.Queries.GetAvailableOffers;
using GearFlow.Modules.Reservations.Application.Queries.GetReservationDraft;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Application.Queries.GetCurrentReservationDraft;
using GearFlow.Modules.Reservations.Application.Queries.GetUpcomingReservationsForUser;
using GearFlow.Modules.Reservations.Domain.Entities;

namespace GearFlow.Api.Controllers.Reservations;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsDraftController : ControllerBase
{
    private const string GetReservationDraftRouteName = "GetReservationDraft";

    private readonly ICommandHandler<CreateDraftReservationCommand> _createDraftHandler;
    private readonly ICommandHandler<AddReservationLineCommand> _addLineHandler;
    private readonly ICommandHandler<RemoveReservationLineCommand> _removeLineHandler;
    private readonly ICommandHandler<ConfirmReservationDraftCommand> _confirmDraftHandler;
    private readonly IQueryHandler<GetAvailableOffers, IEnumerable<AvailableOfferResult>> _getAvailableOffersHandler;
    private readonly IQueryHandler<GetReservationDraft, ReservationDraftDto?> _getReservationDraftHandler;
    private readonly IQueryHandler<GetCurrentReservationDraft, ReservationDraftDto?> _getCurrentReservationDraftHandler;
    private readonly IQueryHandler<GetUpcomingReservations, IEnumerable<UpcomingReservationDto>> _getUpcomingReservationsHandler;

    public ReservationsDraftController(ICommandHandler<CreateDraftReservationCommand> createDraftHandler, ICommandHandler<AddReservationLineCommand> addLineHandler,
        ICommandHandler<RemoveReservationLineCommand> removeLineHandler, ICommandHandler<ConfirmReservationDraftCommand> confirmDraftHandler,
        IQueryHandler<GetAvailableOffers, IEnumerable<AvailableOfferResult>> getAvailableOffersHandler, IQueryHandler<GetReservationDraft,
        ReservationDraftDto?> getReservationDraftHandler, IQueryHandler<GetCurrentReservationDraft, ReservationDraftDto?> getCurrentReservationDraftHandler,
        IQueryHandler<GetUpcomingReservations, IEnumerable<UpcomingReservationDto>> getUpcomingReservationsHandler
        )
    {
        _createDraftHandler = createDraftHandler;
        _addLineHandler = addLineHandler;
        _removeLineHandler = removeLineHandler;
        _confirmDraftHandler = confirmDraftHandler;
        _getAvailableOffersHandler = getAvailableOffersHandler;
        _getReservationDraftHandler = getReservationDraftHandler;
        _getCurrentReservationDraftHandler = getCurrentReservationDraftHandler;
        _getUpcomingReservationsHandler = getUpcomingReservationsHandler;
    }

    [HttpPost("drafts")]
    public async Task<ActionResult> CreateDraftAsync([FromBody] CreateDraftReservationRequest request, CancellationToken cancellationToken)
    {
        var reservationId = Guid.NewGuid();

        var command = new CreateDraftReservationCommand(reservationId, request.TargetCustomerId, request.From, request.To, request.Currency ?? "PLN");

        await _createDraftHandler.HandleAsync(command, cancellationToken);

        return CreatedAtRoute(GetReservationDraftRouteName, new { draftId = reservationId }, new { reservationId });
    }

    [HttpGet("drafts/{draftId:guid}", Name = GetReservationDraftRouteName)]
    public async Task<ActionResult<ReservationDraftDto>> GetReservationDraftAsync([FromRoute] Guid draftId, CancellationToken cancellationToken)
    {
        var draft = await _getReservationDraftHandler.HandleAsync(new GetReservationDraft(draftId), cancellationToken);

        return Ok(draft);
    }

    [HttpGet("drafts/current")]
    public async Task<ActionResult<ReservationDraftDto?>> GetCurrentReservationDraftAsync([FromQuery] GetCurrentReservationDraft query, CancellationToken cancellationToken)
    {
        var draft = await _getCurrentReservationDraftHandler.HandleAsync(query, cancellationToken);
        if (draft is null)
        {
            return NoContent();
        }
        return Ok(draft);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<UpcomingReservationDto>>> GetUpcomingReservationsAsync([FromQuery] GetUpcomingReservations query, CancellationToken cancellationToken)
    {
        var reservations = await _getUpcomingReservationsHandler.HandleAsync(query, cancellationToken);
        
        return Ok(reservations);
    }

    [HttpGet("drafts/current/offers")]
    public async Task<ActionResult<IEnumerable<AvailableOfferDto>>> GetAvailableOffersAsync([FromQuery] GetAvailableOffersRequest request, CancellationToken cancellationToken)
    {
        var currency = CurrencyCode.From(request.Currency ?? "PLN");

        var query = new GetAvailableOffers(
            request.TargetCustomerId,
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

        var offersDto = offers.Select(o => new AvailableOfferDto(
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

    [HttpPost("drafts/current/lines")]
    public async Task<ActionResult<ReservationDraftDto>> AddLineAsync([FromBody] AddReservationLineRequest request, CancellationToken cancellationToken)
    {
        var reservationLineId = Guid.NewGuid();

        var command = new AddReservationLineCommand(reservationLineId, request.OfferVariantId, request.TargetCustomerId);

        await _addLineHandler.HandleAsync(command, cancellationToken);

        var dto = await _getCurrentReservationDraftHandler.HandleAsync(new GetCurrentReservationDraft(request.TargetCustomerId), cancellationToken);

        return Ok(dto);
    }

    [HttpDelete("drafts/current/lines/{lineId:guid}")]
    public async Task<ActionResult<ReservationDraftDto>> RemoveLineAsync([FromRoute] Guid lineId, [FromQuery] Guid? targetCustomerId, CancellationToken cancellationToken)
    {
        var command = new RemoveReservationLineCommand(targetCustomerId, lineId);

        await _removeLineHandler.HandleAsync(command, cancellationToken);

        var dto = await _getCurrentReservationDraftHandler.HandleAsync(new GetCurrentReservationDraft(targetCustomerId), cancellationToken);

        return Ok(dto);
    }

    [HttpPost("drafts/current/confirm")]
    public async Task<ActionResult> ConfirmDraftAsync([FromBody] ConfirmReservationDraftRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmReservationDraftCommand(request.TargetCustomerId, request.PaymentMethod);

        await _confirmDraftHandler.HandleAsync(command, cancellationToken);

        return Ok();
    }

    private static Money? CreateMoney(decimal? amount, CurrencyCode currency)
        => amount.HasValue ? Money.Create(amount.Value, currency) : null;
}

public sealed class ConfirmReservationDraftRequest
{
    public Guid? TargetCustomerId { get; init; }
    public PaymentMethod PaymentMethod { get; set; }
}

public sealed class CreateDraftReservationRequest
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public string? Currency { get; init; } = "PLN";
    public Guid? TargetCustomerId { get; init; }
}

public sealed record GetAvailableOffersRequest
{
    public Guid? TargetCustomerId { get; init; }
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
    public Guid? TargetCustomerId { get; init; }
}

public sealed record AvailableOfferDto(
    Guid VariantId,
    string Brand,
    string Model,
    string Type,
    decimal PricePerDay,
    string Currency,
    string? Size,
    int AvailableCount
    );
