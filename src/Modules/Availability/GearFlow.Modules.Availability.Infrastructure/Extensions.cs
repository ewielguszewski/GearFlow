using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Availability.Infrastructure.Allocators;
using GearFlow.Modules.Availability.Infrastructure.DAL;
using GearFlow.Modules.Availability.Infrastructure.Readers;
using GearFlow.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace GearFlow.Modules.Availability.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddAvailabilityModule(this IServiceCollection services)
    {
        services.AddPostgres<AvailabilityDbContext>();
        services.AddScoped<IAvailabilityReader, AvailabilityReader>();
        services.AddScoped<IAvailabilityAllocator, AvailabilityAllocator>();

        return services;
    }
}