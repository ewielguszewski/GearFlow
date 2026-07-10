namespace GearFlow.Modules.Users.Core.UserContext;

public interface IUserContext
{
    CurrentUser? GetCurrentUser();
}