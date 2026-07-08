using Testcontainers.PostgreSql;

namespace GearFlow.Tests.Integration.Infrastructure;

public sealed class GearFlowIntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("gearflow_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    internal GearFlowApiFactory ApiFactory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        ApiFactory = new GearFlowApiFactory(_postgres.GetConnectionString());
        await ApiFactory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (ApiFactory is not null)
            await ApiFactory.DisposeAsync();

        await _postgres.DisposeAsync();
    }
}
