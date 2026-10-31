using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fcg.Contracts.Events.V1;
using Fcg.Users.Application.Users.RegisterUser;
using Fcg.Users.Application.Users.SeedAdministrator;
using Fcg.Users.Domain.Users;
using Fcg.Users.Infrastructure._Shared.Context;
using Fcg.Users.IntegrationTests._Fixtures;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Serialization;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Users.IntegrationTests.WebApi.Users;

[Collection(UsersApiCollection.Name)]
public class UserCreatedEventPublicationTests
{
    private static readonly string SamplePath = Path.Combine(AppContext.BaseDirectory, "_Fixtures", "samples", "user-created.json");

    private readonly UsersWebApplicationFactory _factory;

    public UserCreatedEventPublicationTests(UsersWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_DevePublicarUserCreatedEventNaExchangeDoContrato_QuandoUsuarioCadastrado()
    {
        var client = _factory.CreateClient();
        var harness = _factory.Services.GetTestHarness();
        var email = UsersApiClient.NewEmail();

        var user = await client.RegisterAsync(email, "Ada Lovelace");

        var received = await harness.WaitForUserCreatedAsync(email);
        received.Should().NotBeNull();
        var published = received!.Context.Message;
        published.UserId.Should().Be(user.Id);
        published.Name.Should().Be("Ada Lovelace");
        published.Email.Should().Be(email);
        published.EventId.Version.Should().Be(7);
        published.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
        published.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        harness.Sent.Select<object>(message => message.Context.DestinationAddress!.AbsolutePath.EndsWith(TestHarnessExtensions.UserCreatedExchange, StringComparison.Ordinal))
            .Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_NaoDevePublicarNovoEvento_QuandoEmailDuplicado()
    {
        var client = _factory.CreateClient();
        var harness = _factory.Services.GetTestHarness();
        var email = UsersApiClient.NewEmail();
        await client.RegisterAsync(email);
        (await harness.WaitForUserCreatedAsync(email)).Should().NotBeNull();

        var duplicate = await client.PostAsJsonAsync("/api/v1/auth/register", new { name = "Outra", email, password = UsersApiClient.DefaultPassword });
        await Task.Delay(TimeSpan.FromSeconds(1));

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
        harness.CountUserCreated(email).Should().Be(1);
    }

    [Fact]
    public async Task Register_DeveGravarEventoNoOutboxNaMesmaTransacao_ENaoPublicar_QuandoTransacaoDesfeita()
    {
        _factory.CreateClient();
        var harness = _factory.Services.GetTestHarness();
        var email = UsersApiClient.NewEmail();
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            await scope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new RegisterUserCommand("Ada Lovelace", email, UsersApiClient.DefaultPassword));

            var userSaved = await context.Set<UserPersistenceModel>().AsNoTracking().AnyAsync(user => user.Email == email);
            var eventInOutbox = await context.Set<OutboxMessage>().AsNoTracking().AnyAsync(message => message.Body.Contains(email));

            userSaved.Should().BeTrue();
            eventInOutbox.Should().BeTrue();

            await transaction.RollbackAsync();
        }

        (await harness.WaitForUserCreatedAsync(email, TimeSpan.FromSeconds(2))).Should().BeNull();
    }

    [Fact]
    public async Task Seed_DevePublicarUserCreatedEventDoAdministrador_QuandoBancoNovo()
    {
        _factory.CreateClient();
        var harness = _factory.Services.GetTestHarness();

        var received = await harness.WaitForUserCreatedAsync(SeedAdministratorCommand.DefaultEmail);

        received.Should().NotBeNull();
        received!.Context.Message.Name.Should().Be(SeedAdministratorCommand.DefaultName);
        harness.CountUserCreated(SeedAdministratorCommand.DefaultEmail).Should().Be(1);
    }

    [Fact]
    public async Task Seed_NaoDevePublicarNovoEventoNemDuplicarAdministrador_QuandoAplicacaoReiniciada()
    {
        _factory.CreateClient();
        (await _factory.Services.GetTestHarness().WaitForUserCreatedAsync(SeedAdministratorCommand.DefaultEmail)).Should().NotBeNull();

        await using var restarted = _factory.WithWebHostBuilder(_ => { });
        restarted.CreateClient();

        (await restarted.Services.GetTestHarness().WaitForUserCreatedAsync(SeedAdministratorCommand.DefaultEmail, TimeSpan.FromSeconds(2)))
            .Should().BeNull();

        await using var scope = restarted.Services.CreateAsyncScope();
        var administrators = await scope.ServiceProvider.GetRequiredService<UsersDbContext>()
            .Set<UserPersistenceModel>()
            .CountAsync(user => user.Email == SeedAdministratorCommand.DefaultEmail);
        administrators.Should().Be(1);
    }

    [Fact]
    public async Task UserCreatedEvent_DeveTerMesmasPropriedadesDoSampleDoContrato_QuandoTrafegadoNoBroker()
    {
        var client = _factory.CreateClient();
        var harness = _factory.Services.GetTestHarness();
        var email = UsersApiClient.NewEmail();
        var user = await client.RegisterAsync(email);
        var received = await harness.WaitForUserCreatedAsync(email);
        received.Should().NotBeNull();

        // Corpo bruto recebido do transporte (envelope do MassTransit); o evento fica na propriedade "message".
        var envelope = JsonNode.Parse(received!.Context.ReceiveContext.Body.GetString())!.AsObject();
        var message = envelope["message"]!.AsObject();
        var sample = JsonNode.Parse(await File.ReadAllTextAsync(SamplePath))!.AsObject();

        message.Select(property => property.Key).Should().Equal(sample.Select(property => property.Key));
        message["userId"]!.GetValue<string>().Should().Be(user.Id.ToString());
        message["name"]!.GetValue<string>().Should().Be(user.Name);
        message["email"]!.GetValue<string>().Should().Be(email);
        message["occurredAt"]!.GetValue<string>().Should().EndWith("Z");
        envelope["messageType"]!.AsArray().Select(type => type!.GetValue<string>()).Should().Contain("urn:message:fcg:user-created:v1");
    }

    [Fact]
    public async Task SampleDoContrato_DeveDesserializarComoUserCreatedEvent_QuandoLidoPeloSerializadorDoMassTransit()
    {
        var sample = JsonSerializer.Deserialize<UserCreatedEvent>(await File.ReadAllTextAsync(SamplePath), SystemTextJsonMessageSerializer.Options);

        sample.Should().Be(new UserCreatedEvent(
            Guid.Parse("0199a1b2-0000-7000-8000-000000000001"),
            new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc),
            Guid.Parse("0199a1b2-0000-7000-8000-0000000000a1"),
            "Ada Lovelace",
            "ada@fcg.dev"));
    }
}
