using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Infrastructure.Postgres.Decorators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

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

    internal static IServiceCollection AddPostgresConnection(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider =>
        {
            var postgresOptions = serviceProvider
                .GetRequiredService<IOptions<PostgresOptions>>()
                .Value;

            return NpgsqlDataSource.Create(postgresOptions.ConnectionString);
        });

        services.AddScoped(serviceProvider =>
            serviceProvider
                .GetRequiredService<NpgsqlDataSource>()
                .CreateConnection());

        return services;
    }

    public static IServiceCollection AddPostgres<T>(this IServiceCollection services) where T : DbContext
    {
        services.AddDbContext<T>((serviceProvider, optionsBuilder) =>
            {
                var connection = serviceProvider.GetRequiredService<NpgsqlConnection>();
                optionsBuilder.UseNpgsql(connection);
            });

        services.AddScoped<DbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<T>());
        
        return services;
    }

    internal static IServiceCollection AddUoWHandlersDecorators(this IServiceCollection services)
    {
        services.TryDecorate(typeof(ICommandHandler<>), typeof(UnitOfWorkCommandHandlerDecorator<>));

        return services;
    }
}
