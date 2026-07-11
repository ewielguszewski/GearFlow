using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Reservations.Application.Exceptions;

public class CurrentUserNotFoundException : UnauthorizedException
{
    public CurrentUserNotFoundException() : base("Current user was not found")
    {
    }
}
