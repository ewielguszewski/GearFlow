namespace GearFlow.Shared.Abstractions.Common;

public class AppException : Exception
{
    public AppException(string message) : base(message) { }

    public AppException(string message, string paramName) : base($"{message} (Parameter: {paramName})")
    {
    }
}
