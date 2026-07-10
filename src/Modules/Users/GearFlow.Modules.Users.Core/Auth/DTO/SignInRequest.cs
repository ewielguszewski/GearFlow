namespace GearFlow.Modules.Users.Core.Auth.DTO;

public class SignInRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
