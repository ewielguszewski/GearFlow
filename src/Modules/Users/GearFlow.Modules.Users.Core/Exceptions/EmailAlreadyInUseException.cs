using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Users.Core.Exceptions;

internal class EmailAlreadyInUseException : DomainException
{
    public EmailAlreadyInUseException(string email) : base($"The email '{email}' is already in use.")
    {
    }
}