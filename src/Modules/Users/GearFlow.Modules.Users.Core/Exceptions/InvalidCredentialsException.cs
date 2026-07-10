using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Users.Core.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() : base("Invalid email or password.")
    {
    }
}
