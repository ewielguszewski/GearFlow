using GearFlow.Modules.Users.Core.Enums;
using GearFlow.Modules.Users.Core.UserContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GearFlow.Modules.Users.Core.Auth;

internal static class Extensions
{
    public static IServiceCollection AddAuthOptions(this IServiceCollection services)
    {
        services.AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Issuer)
                && !string.IsNullOrWhiteSpace(options.Audience)
                && !string.IsNullOrWhiteSpace(options.SigningKey)
                && options.ExpiryRefreshToken > TimeSpan.Zero
                && options.Expiry > TimeSpan.Zero,
                "Auth options are missing.")
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new AuthOptions();
        var section = configuration.GetRequiredSection(AuthOptions.SectionName);
        section.Bind(options);

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.Audience = options.Audience;
                o.IncludeErrorDetails = true;
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,

                    ValidateAudience = true,
                    ValidAudience = options.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(options.SigningKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = UserClaims.Role
                };
            });

        services.AddAuthorization(o =>
        {
            o.AddPolicy("Admin", p =>
                p.RequireRole(Role.Admin.ToString()));

            o.AddPolicy("EmployeeOrAdmin", p =>
                p.RequireRole(Role.Employee.ToString(), Role.Admin.ToString()));

            o.AddPolicy("Customer", p =>
                p.RequireRole(Role.Customer.ToString()));
        });

        return services;
    }
}
