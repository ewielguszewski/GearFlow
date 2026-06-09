namespace GearFlow.Shared.Abstractions.Common;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, string paramName) : base($"{message} (Parameter: {paramName})")
    {
    }
}
