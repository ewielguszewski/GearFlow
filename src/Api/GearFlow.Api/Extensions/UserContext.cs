using GearFlow.Modules.Users.Core.Enums;
using GearFlow.Modules.Users.Core.UserContext;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GearFlow.Api.Extensions;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser? GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return null;

        var roleClaim = principal.FindFirst(UserClaims.Role)?.Value;
        if (!Enum.TryParse<Role>(roleClaim, ignoreCase: true, out var role))
            return null;

        var customerIdClaim = principal.FindFirst(UserClaims.CustomerId)?.Value;
        Guid? customerId = null;
        if (Guid.TryParse(customerIdClaim, out var parsedCustomerId))
            customerId = parsedCustomerId;

        return new CurrentUser(userId, customerId, role);
    }
}
