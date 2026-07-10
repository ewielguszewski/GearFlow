using GearFlow.Modules.Users.Core.Auth.DTO;

namespace GearFlow.Modules.Users.Core.Auth;

public interface IAuthService
{
    Task<AuthResponse> SignUpAsync(SignUpRequest dto, CancellationToken cancellationToken);
    Task<AuthResponse> SignInAsync(SignInRequest dto, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest dto, CancellationToken cancellationToken);
}
