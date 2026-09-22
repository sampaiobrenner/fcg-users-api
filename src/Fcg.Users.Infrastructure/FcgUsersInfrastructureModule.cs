using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Messaging;
using Fcg.Users.Domain._Shared.Modules;
using Fcg.Users.Infrastructure._Shared.Context;
using Fcg.Users.Infrastructure._Shared.Messaging;
using Fcg.Users.Infrastructure.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Fcg.Users.Infrastructure;

public sealed class FcgUsersInfrastructureModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(InfrastructureResources.ConnectionStringAusente);

        var npgsqlConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            GssEncryptionMode = GssEncryptionMode.Disable
        }.ConnectionString;

        services.AddDbContext<UsersDbContext>(options => options
            .UseNpgsql(npgsqlConnectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUsersDbContext>(provider => provider.GetRequiredService<UsersDbContext>());
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
    }
}
