namespace GearFlow.Modules.Users.Core.Auth.DTO;

public class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}
