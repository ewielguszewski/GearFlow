using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Modules.Reservations.Infrastructure.DAL;
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

        return services;
    }
}
