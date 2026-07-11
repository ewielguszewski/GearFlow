using GearFlow.Shared.Infrastructure.Postgres;
using GearFlow.Modules.Users.Core.Auth;
using GearFlow.Modules.Users.Core.DAL;
using GearFlow.Modules.Users.Core.DAL.Repositories;
using GearFlow.Modules.Users.Core.Repositories;
using GearFlow.Modules.Users.Core.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GearFlow.Modules.Users.Core.DAL.Seeding;

namespace GearFlow.Modules.Users.Core;

public static class Extensions
{
    public static IServiceCollection AddUsersCore(this IServiceCollection services, IConfiguration configuration)
        => services
        .AddScoped<IUserRepository, UserRepository>()
        .AddScoped<ICustomerRepository, CustomerRepository>()
        .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
        .AddSecurity()
        .AddPostgres<UsersDbContext>()
        .AddScoped<UserDbSeeder>()
        .AddAuthOptions()
        .AddAuth(configuration);
}
