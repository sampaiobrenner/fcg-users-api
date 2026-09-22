using System.Net;
using Fcg.Users.IntegrationTests._Fixtures;

namespace Fcg.Users.IntegrationTests.WebApi._Shared.HealthChecks;

public class HealthEndpointsTests : IClassFixture<UsersWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(UsersWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Live_DeveRetornarOk_QuandoAplicacaoEmExecucao()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApi_DeveRetornarDocumento_QuandoSolicitado()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
