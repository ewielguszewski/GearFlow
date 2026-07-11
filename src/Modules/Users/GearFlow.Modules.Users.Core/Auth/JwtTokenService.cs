using GearFlow.Modules.Users.Core.Entities;
using GearFlow.Shared.Abstractions.Security;
using GearFlow.Shared.Abstractions.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace GearFlow.Modules.Users.Core.Auth;

internal class JwtTokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _expiry;
    private readonly SigningCredentials _signingCredentials;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler = new();
    private readonly IClock _clock;

    public JwtTokenService(IOptions<AuthOptions> options, IClock clock)
    {
        _issuer = options.Value.Issuer;
        _audience = options.Value.Audience;
        _expiry = options.Value.Expiry;
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(options.Value.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        _clock = clock;
    }

    public string GenerateAccessToken(UserAccount user)
    {
        var now = _clock.Current();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(UserClaims.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.CustomerId.HasValue)
            claims.Add(new Claim(UserClaims.CustomerId, user.CustomerId.Value.ToString()));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            Expires = now.Add(_expiry),
            SigningCredentials = _signingCredentials
        };

        return _jsonWebTokenHandler.CreateToken(descriptor);
    }
}
