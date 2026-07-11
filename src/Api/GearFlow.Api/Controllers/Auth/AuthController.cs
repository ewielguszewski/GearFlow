using GearFlow.Modules.Users.Core.Auth;
using GearFlow.Modules.Users.Core.Auth.DTO;
using GearFlow.Shared.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearFlow.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserContext _userContext;

    public AuthController(IAuthService authService, IUserContext userContext)
    {
        _authService = authService;
        _userContext = userContext;
    }

    [HttpPost("sign-up")]
    public async Task<ActionResult<AuthResponse>> SignUpAsync([FromBody] SignUpRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.SignUpAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("sign-in")]
    public async Task<ActionResult<AuthResponse>> SignInAsync([FromBody] SignInRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.SignInAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<MeResponse> GetMe() 
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser is null)
            return Unauthorized();

        return Ok(new MeResponse(
            currentUser.UserId,
            currentUser.CustomerId,
            currentUser.Role.ToString()));
    }
}

public sealed record MeResponse(
    Guid UserId,
    Guid? CustomerId,
    string Role);
