namespace GearFlow.Shared.Abstractions.Common;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, string paramName) : base($"{message} (Parameter: {paramName})")
    {
    }
}
