using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Application.Queries;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Modules.Reservations.Infrastructure.Authorization.Services;
using GearFlow.Modules.Reservations.Infrastructure.Background;
using GearFlow.Modules.Reservations.Infrastructure.DAL;
using GearFlow.Modules.Reservations.Infrastructure.DAL.Readers;
using GearFlow.Modules.Reservations.Infrastructure.DAL.Repositories;
using GearFlow.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace GearFlow.Modules.Reservations.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddReservationsModule(this IServiceCollection services)
    {
        services.AddPostgres<ReservationsDbContext>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IExpiredDraftReservationProcessor, ExpiredDraftReservationProcessor>();
        services.AddHostedService<ExpiredDraftReservationCleanupWorker>();
        services.AddScoped<IReservationAuthorizationService, ReservationAuthorizationService>();
        services.AddScoped<IReservationReader, ReservationReader>();

        services.AddOptions<ReservationExpiryCleanupOptions>()
            .BindConfiguration(ReservationExpiryCleanupOptions.SectionName)
            .Validate(options => options.IntervalSeconds > 0, "Reservation expiry cleanup interval must be greater than zero.")
            .Validate(options => options.BatchSize > 0, "Reservation expiry cleanup batch size must be greater than zero.")
            .Validate(options => options.InitialDelaySeconds >= 0, "Reservation expiry cleanup initial delay cannot be negative.")
            .ValidateOnStart();

        return services;
    }
}
