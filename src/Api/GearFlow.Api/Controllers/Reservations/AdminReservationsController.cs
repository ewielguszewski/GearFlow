using GearFlow.Modules.Reservations.Application.Queries.BrowseAdminReservations;
using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Users.Core.Policies;
using GearFlow.Shared.Abstractions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearFlow.Api.Controllers.Reservations;

[ApiController]
[Route("api/admin/reservations")]
[Authorize(Policy = AuthorizationPolicies.EmployeeOrAdmin)]
public sealed class AdminReservationsController : ControllerBase
{
    private readonly IQueryHandler<BrowseAdminReservations, IEnumerable<AdminReservationDto>> _browseReservationsHandler;

    public AdminReservationsController(IQueryHandler<BrowseAdminReservations, IEnumerable<AdminReservationDto>> browseReservationsHandler)
    {
        _browseReservationsHandler = browseReservationsHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminReservationDto>>> BrowseReservationsAsync([FromQuery] BrowseAdminReservationsRequest request, CancellationToken cancellationToken)
    {
        var query = new BrowseAdminReservations(
            request.CustomerId,
            request.Status,
            request.CancellationReason,
            request.From,
            request.To,
            request.PickupState);

        var reservations = await _browseReservationsHandler.HandleAsync(query, cancellationToken);

        return Ok(reservations);
    }
}

public sealed class BrowseAdminReservationsRequest
{
    public Guid? CustomerId { get; init; }
    public ReservationStatus? Status { get; init; }
    public CancellationReason? CancellationReason { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public ReservationPickupState? PickupState { get; init; }
}
