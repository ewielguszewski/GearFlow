using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Infrastructure.Postgres.Decorators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GearFlow.Shared.Infrastructure.Postgres;

public static class Extensions
{
    internal static IServiceCollection AddPostgresOptions(this IServiceCollection services)
    {
        services.AddOptions<PostgresOptions>()
            .BindConfiguration(PostgresOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Postgres connection cannot be empty.")
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddPostgres<T>(this IServiceCollection services) where T : DbContext
    {
        services.AddDbContext<T>((serviceProvider, optionsBuilder) =>
            {
                var postgresOptions = serviceProvider
                    .GetRequiredService<IOptions<PostgresOptions>>()
                    .Value;

                optionsBuilder.UseNpgsql(postgresOptions.ConnectionString);
            });
        
        return services;
    }

    public static IServiceCollection AddUoWHandlersDecorators(this  IServiceCollection services)
    {
        services.TryDecorate(typeof(ICommandHandler<>), typeof(UnitOfWorkCommandHandlerDecorator<>));

        return services;
    }
}
