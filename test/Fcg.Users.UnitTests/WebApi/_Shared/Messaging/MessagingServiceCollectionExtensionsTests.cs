using Fcg.Users.Infrastructure._Shared.Context;
using Fcg.Users.WebApi._Shared.Messaging;
using MassTransit;
using MassTransit.DependencyInjection;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Users.UnitTests.WebApi._Shared.Messaging;

public class MessagingServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddMessaging_DevePublicarViaOutboxDoEntityFramework_QuandoForaDeUmConsumer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<UsersDbContext>(options => options.UseNpgsql("Host=localhost;Database=fcg_users;Username=fcg;Password=fcg"));
        services.AddMessaging(new ConfigurationBuilder().Build());

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        // Publicacoes feitas por endpoints HTTP e pelo seed passam pelo bus outbox (gravadas em outbox_message).
        scope.ServiceProvider.GetRequiredService<IScopedBusContextProvider<IBus>>()
            .Should().BeOfType<EntityFrameworkScopedBusContextProvider<IBus, UsersDbContext>>();
    }
}
