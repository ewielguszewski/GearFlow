using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Infrastructure.Commands;
using GearFlow.Shared.Infrastructure.Exceptions;
using GearFlow.Shared.Infrastructure.Logging;
using GearFlow.Shared.Infrastructure.Postgres;
using GearFlow.Shared.Infrastructure.Queries;
using GearFlow.Shared.Infrastructure.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GearFlow.Shared.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IList<Assembly> assemblies)
    {
        services.AddTransient<ErrorHandlerMiddleware>();
        services.AddSingleton<IClock, UtcClock>();
        services.AddCommands(assemblies);
        services.AddQueries(assemblies);
        services.AddLoggingDecorators();
        services.AddPostgresOptions();
        services.AddPostgresConnection();
        services.AddScoped<IUnitOfWork, EfPostgresUnitOfWork>();
        services.AddUoWHandlersDecorators();
        services.AddLoggingDecorators();

        return services;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlerMiddleware>();
        return app;
    }
}
