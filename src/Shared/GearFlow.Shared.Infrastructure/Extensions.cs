using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Infrastructure.Commands;
using GearFlow.Shared.Infrastructure.Postgres;
using GearFlow.Shared.Infrastructure.Queries;
using GearFlow.Shared.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GearFlow.Shared.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IList<Assembly> assemblies)
    {
        services.AddSingleton<IClock, UtcClock>();
        services.AddCommands(assemblies);
        services.AddQueries(assemblies);
        services.AddPostgresOptions();
        services.AddPostgresConnection();
        services.AddUoWHandlersDecorators();
        services.AddScoped<IUnitOfWork, EfPostgresUnitOfWork>();

        return services;
    }
}
