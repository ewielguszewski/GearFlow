using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Shared.Abstractions.Security;

namespace GearFlow.Modules.Reservations.Infrastructure.Authorization.Services;

public class ReservationAuthorizationService : IReservationAuthorizationService
{
    private readonly IUserContext _userContext;

    public ReservationAuthorizationService(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public void Authorize(Reservation reservation)
    {
        var user = GetRequiredUser();

        if (user.IsEmployeeOrAdmin())
            return;
        if (user.IsCustomer() && user.CustomerId == reservation.CustomerId)
            return;

        throw new ForbiddenException();
    }

    public Guid ResolveCustomerId(Guid? requestedCustomerId)
    {
        var user = GetRequiredUser();

        if (user.IsCustomer())
        {
            if (requestedCustomerId.HasValue && requestedCustomerId != user.CustomerId)
                throw new ForbiddenException();

            return (Guid)user.CustomerId!;
        }

        if (user.IsEmployeeOrAdmin())
        {
            if (!requestedCustomerId.HasValue)
                throw new DomainException("CustomerId is required for employee flow");

            return requestedCustomerId.Value;
        }

        throw new ForbiddenException();
    }

    private CurrentUser GetRequiredUser()
    {
        var user = _userContext.GetCurrentUser();
        if (user == null)
            throw new UnauthorizedException("User is not authenticated.");

        return user;
    }
}
