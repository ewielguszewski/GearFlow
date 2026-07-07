using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Catalog.Infrastructure.DAL;
using GearFlow.Modules.Catalog.Infrastructure.Readers;
using GearFlow.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace GearFlow.Modules.Catalog.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddPostgres<CatalogDbContext>();
        services.AddScoped<ICatalogOfferReader, CatalogOfferReader>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICatalogAdminService, CatalogAdminService>();
        services.AddScoped<ICatalogBrowsingService, CatalogBrowsingService>();
        services.AddScoped<ICatalogBrowsingReader, CatalogBrowsingReader>();

        return services;
    }
}
