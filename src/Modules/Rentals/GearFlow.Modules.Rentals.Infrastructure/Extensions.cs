using GearFlow.Modules.Rentals.Application.Queries;
using GearFlow.Modules.Rentals.Domain.Repositories;
using GearFlow.Modules.Rentals.Infrastructure.DAL;
using GearFlow.Modules.Rentals.Infrastructure.DAL.Readers;
using GearFlow.Modules.Rentals.Infrastructure.DAL.Repositories;
using GearFlow.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace GearFlow.Modules.Rentals.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddRentalsModule(this IServiceCollection services)
    {
        services.AddPostgres<RentalsDbContext>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IRentalReader, RentalReader>();

        return services;
    }
}
