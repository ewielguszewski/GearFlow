using GearFlow.Tests.Integration.Infrastructure;
using System.Net;

namespace GearFlow.Tests.Integration.HealthChecks;

[Collection(GearFlowIntegrationCollection.Name)]
public class HealthCheckEndpoints_Tests : IClassFixture<GearFlowIntegrationFixture>
{
    private readonly GearFlowIntegrationFixture _fixture;

    public HealthCheckEndpoints_Tests(GearFlowIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task health_endpoint_should_return_ok_without_authentication(string endpoint)
    {
        using var client = _fixture.ApiFactory.CreateClient();

        var response = await client.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
