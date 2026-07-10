using GearFlow.Modules.Availability.Infrastructure.DAL;
using GearFlow.Modules.Catalog.Infrastructure.DAL;
using GearFlow.Modules.Catalog.Infrastructure.DAL.Seeding;
using GearFlow.Modules.Reservations.Infrastructure.DAL;
using GearFlow.Modules.Users.Core.DAL;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Api.Extensions;

internal static class DevelopmentDatabaseInitializer
{
    public static async Task InitializeDevelopmentDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        await serviceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
        await serviceProvider.GetRequiredService<AvailabilityDbContext>().Database.MigrateAsync();
        await serviceProvider.GetRequiredService<ReservationsDbContext>().Database.MigrateAsync();
        await serviceProvider.GetRequiredService<UsersDbContext>().Database.MigrateAsync();

        await serviceProvider
            .GetRequiredService<CatalogDbSeeder>()
            .SeedAsync();
    }
}
