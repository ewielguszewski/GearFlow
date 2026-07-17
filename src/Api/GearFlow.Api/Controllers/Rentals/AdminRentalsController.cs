using GearFlow.Modules.Rentals.Application.Commands.CompleteRentalReturn;
using GearFlow.Modules.Rentals.Application.Commands.StartRentalFromReservation;
using GearFlow.Modules.Rentals.Application.Queries.BrowseRentals;
using GearFlow.Modules.Rentals.Application.Queries.DTO;
using GearFlow.Modules.Rentals.Application.Queries.GetRental;
using GearFlow.Modules.Rentals.Domain.Enums;
using GearFlow.Modules.Users.Core.Policies;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearFlow.Api.Controllers.Rentals;

[ApiController]
[Route("api/admin/rentals")]
[Authorize(Policy = AuthorizationPolicies.EmployeeOrAdmin)]
public sealed class AdminRentalsController : ControllerBase
{
    private const string GetRentalRouteName = "GetRental";

    private readonly ICommandHandler<StartRentalFromReservationCommand> _startRentalHandler;
    private readonly ICommandHandler<CompleteRentalReturnCommand> _completeReturnHandler;
    private readonly IQueryHandler<GetRental, RentalDto?> _getRentalHandler;
    private readonly IQueryHandler<BrowseRentals, IEnumerable<RentalDto>> _browseRentalsHandler;

    public AdminRentalsController(
        ICommandHandler<StartRentalFromReservationCommand> startRentalHandler,
        ICommandHandler<CompleteRentalReturnCommand> completeReturnHandler,
        IQueryHandler<GetRental, RentalDto?> getRentalHandler,
        IQueryHandler<BrowseRentals, IEnumerable<RentalDto>> browseRentalsHandler)
    {
        _startRentalHandler = startRentalHandler;
        _completeReturnHandler = completeReturnHandler;
        _getRentalHandler = getRentalHandler;
        _browseRentalsHandler = browseRentalsHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RentalDto>>> BrowseRentalsAsync([FromQuery] BrowseAdminRentalsRequest request, CancellationToken cancellationToken)
    {
        var query = new BrowseRentals(
            request.CustomerId,
            request.ReservationId,
            request.Status,
            request.From,
            request.To,
            request.LifecycleState);

        var rentals = await _browseRentalsHandler.HandleAsync(query, cancellationToken);

        return Ok(rentals);
    }

    [HttpGet("{rentalId:guid}", Name = GetRentalRouteName)]
    public async Task<ActionResult<RentalDto>> GetRentalAsync([FromRoute] Guid rentalId, CancellationToken cancellationToken)
    {
        var rental = await _getRentalHandler.HandleAsync(new GetRental(rentalId), cancellationToken);

        return rental is null ? NotFound() : Ok(rental);
    }

    [HttpPost("from-reservation/{reservationId:guid}")]
    public async Task<ActionResult<RentalDto>> StartRentalFromReservationAsync([FromRoute] Guid reservationId, [FromBody] StartRentalFromReservationRequest request, 
        CancellationToken cancellationToken)
    {
        var rentalId = Guid.NewGuid();
        var lines = request.Lines ?? [];

        var command = new StartRentalFromReservationCommand(
            rentalId,
            reservationId,
            lines.Select(x => new RentalLinePickupInput(
                x.ReservationLineId,
                x.Condition,
                x.ConditionNote)).ToArray());

        await _startRentalHandler.HandleAsync(command, cancellationToken);

        var rental = await _getRentalHandler.HandleAsync(new GetRental(rentalId), cancellationToken);

        return CreatedAtRoute(GetRentalRouteName, new { rentalId }, rental);
    }

    [HttpPost("{rentalId:guid}/return")]
    public async Task<ActionResult<RentalDto>> CompleteRentalReturnAsync([FromRoute] Guid rentalId, [FromBody] CompleteRentalReturnRequest request,
        CancellationToken cancellationToken)
    {
        var lines = request.Lines ?? [];

        var command = new CompleteRentalReturnCommand(
            rentalId,
            lines.Select(x => new RentalLineReturnInput(
                x.RentalLineId,
                x.Condition,
                x.Note,
                x.DamageFeeAmount)).ToArray(),
            request.LateFeeAmount);

        await _completeReturnHandler.HandleAsync(command, cancellationToken);

        var rental = await _getRentalHandler.HandleAsync(new GetRental(rentalId), cancellationToken);

        return Ok(rental);
    }
}

public sealed class BrowseAdminRentalsRequest
{
    public Guid? CustomerId { get; init; }
    public Guid? ReservationId { get; init; }
    public RentalStatus? Status { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public RentalLifecycleState? LifecycleState { get; init; }
}

public sealed class StartRentalFromReservationRequest
{
    public IReadOnlyCollection<RentalLinePickupRequest> Lines { get; init; } = [];
}

public sealed class RentalLinePickupRequest
{
    public Guid ReservationLineId { get; init; }
    public ItemCondition Condition { get; init; } = ItemCondition.Good;
    public string? ConditionNote { get; init; }
}

public sealed class CompleteRentalReturnRequest
{
    public decimal LateFeeAmount { get; init; }

    public IReadOnlyCollection<RentalLineReturnRequest> Lines { get; init; } = [];
}

public sealed class RentalLineReturnRequest
{
    public Guid RentalLineId { get; init; }
    public ItemCondition Condition { get; init; }
    public string? Note { get; init; }
    public decimal DamageFeeAmount { get; init; }
}
