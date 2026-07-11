namespace GearFlow.Shared.Abstractions.Common;

public class ForbiddenException : AppException
{
    public ForbiddenException() : base("You do not have access to this content")
    {
    }

    public ForbiddenException(string message, string paramName) : base($"{message} (Parameter: {paramName})")
    {
    }
}
