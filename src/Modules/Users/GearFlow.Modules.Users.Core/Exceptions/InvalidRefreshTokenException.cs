using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Users.Core.Exceptions;

public class InvalidRefreshTokenException : UnauthorizedException
{
    public InvalidRefreshTokenException() : base("Invalid refresh token.")
    {
    }
}
