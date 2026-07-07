namespace GearFlow.Shared.Abstractions.Common;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message)
    {
    }
}
