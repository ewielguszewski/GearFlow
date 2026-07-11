using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Security;

namespace GearFlow.Modules.Reservations.Application.Interfaces;

public interface IReservationAuthorizationService
{
    void Authorize(Reservation reservation);
    Guid ResolveCustomerId(Guid? requestedCustomerId);
}
