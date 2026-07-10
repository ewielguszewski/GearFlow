using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Users.Core.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(Guid userId) : base($"User with id '{userId}' was not found.")
    {
    }
}
