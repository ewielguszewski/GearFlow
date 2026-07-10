using GearFlow.Modules.Users.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GearFlow.Modules.Users.Core.Security;

internal static class Extensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services
            .AddSingleton<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>()
            .AddSingleton<IPasswordManager, PasswordManager>();

        return services;
    }
}
