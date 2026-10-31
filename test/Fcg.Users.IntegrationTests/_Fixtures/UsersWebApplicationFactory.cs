using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Testcontainers.PostgreSql;

namespace Fcg.Users.IntegrationTests._Fixtures;

/// <summary>
/// Sobe a API contra um PostgreSQL real (Testcontainers), aplicando as migrations e o seed no startup,
/// com o RabbitMQ substituido pelo test harness em memoria do MassTransit.
/// </summary>
public sealed class UsersWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string JwtKey = "fcg-integration-tests-signing-key-0123456789";
    public const string JwtIssuer = "fcg-users-api";
    public const string JwtAudience = "fcg";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("fcg_users")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("Jwt:Key", JwtKey);

        builder.ConfigureTestServices(services => services.AddMassTransitTestHarness());
    }
}
