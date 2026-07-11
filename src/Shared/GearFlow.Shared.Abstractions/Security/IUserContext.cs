namespace GearFlow.Shared.Abstractions.Security;

public interface IUserContext
{
    CurrentUser? GetCurrentUser();
}