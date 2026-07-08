using GearFlow.Modules.Availability.Infrastructure.DAL;
using GearFlow.Modules.Catalog.Infrastructure.DAL;
using GearFlow.Modules.Catalog.Infrastructure.DAL.Seeding;
using GearFlow.Modules.Reservations.Infrastructure.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GearFlow.Tests.Integration.Infrastructure;

internal sealed class GearFlowApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public GearFlowApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["postgres:connectionString"] = _connectionString
            });
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        await serviceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
        await serviceProvider.GetRequiredService<AvailabilityDbContext>().Database.MigrateAsync();
        await serviceProvider.GetRequiredService<ReservationsDbContext>().Database.MigrateAsync();
        await serviceProvider.GetRequiredService<CatalogDbSeeder>().SeedAsync();
    }
}
