namespace GearFlow.Modules.Users.Core.Auth.DTO;

public class LogoutRequest
{
    public string RefreshToken { get; init; } = default!;
}
