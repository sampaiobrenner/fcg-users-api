using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fcg.Contracts.Security;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using Fcg.Users.IntegrationTests._Fixtures;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fcg.Users.IntegrationTests.WebApi.Auth;

[Collection(UsersApiCollection.Name)]
public class AuthFlowTests
{
    private readonly UsersWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthFlowTests(UsersWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterLoginMe_DeveRetornarPerfilDoToken_QuandoFluxoCompleto()
    {
        var email = UsersApiClient.NewEmail();

        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { name = "Ada Lovelace", email, password = UsersApiClient.DefaultPassword });
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await register.Content.ReadFromJsonAsync<UserResponse>(UsersApiClient.JsonOptions))!;
        register.Headers.Location!.ToString().Should().Be($"/api/v1/users/{created.Id}");
        created.Role.Should().Be(UserRole.User);
        created.Active.Should().BeTrue();

        var login = await _client.LoginAsync(email);
        login.UserId.Should().Be(created.Id);
        login.Email.Should().Be(email);
        login.Role.Should().Be(UserRole.User);
        login.ExpiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var me = await _client.SendAsync(request);

        me.StatusCode.Should().Be(HttpStatusCode.OK);
        (await me.Content.ReadFromJsonAsync<UserResponse>(UsersApiClient.JsonOptions)).Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Login_DeveEmitirTokenComClaimsDoContrato_QuandoCredenciaisValidas()
    {
        var email = UsersApiClient.NewEmail();
        var user = await _client.RegisterAsync(email, "Grace Hopper");

        var token = new JsonWebTokenHandler().ReadJsonWebToken((await _client.LoginAsync(email)).AccessToken);

        token.Issuer.Should().Be(UsersWebApplicationFactory.JwtIssuer);
        token.Audiences.Should().ContainSingle().Which.Should().Be(UsersWebApplicationFactory.JwtAudience);
        token.GetClaim(FcgClaimTypes.UserId).Value.Should().Be(user.Id.ToString());
        token.GetClaim(FcgClaimTypes.Email).Value.Should().Be(email);
        token.GetClaim(FcgClaimTypes.Name).Value.Should().Be("Grace Hopper");
        token.GetClaim(FcgClaimTypes.Role).Value.Should().Be(FcgRoles.User);
        token.GetClaim(JwtRegisteredClaimNames.Jti).Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_DeveRetornarConflito_QuandoEmailJaCadastrado()
    {
        var email = UsersApiClient.NewEmail();
        await _client.RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { name = "Outra", email = email.ToUpperInvariant(), password = UsersApiClient.DefaultPassword });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("", "valid@fcg.dev", "Str0ng!Pass")]
    [InlineData("Ada", "invalido", "Str0ng!Pass")]
    [InlineData("Ada", "valid@fcg.dev", "fraca")]
    public async Task Register_DeveRetornarBadRequest_QuandoDadosInvalidos(string name, string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new { name, email, password });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_DeveRetornarNaoAutorizado_QuandoSenhaErrada()
    {
        var email = UsersApiClient.NewEmail();
        await _client.RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Errada123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_DeveRetornarNaoAutorizado_QuandoEmailDesconhecido()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = UsersApiClient.NewEmail(), password = UsersApiClient.DefaultPassword });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_DeveRetornarNaoAutorizado_QuandoSemToken()
    {
        var response = await _client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_DeveFuncionarParaAdministradorPadrao_QuandoSeedExecutado()
    {
        var admin = await _factory.CreateAdministratorClientAsync();

        var me = await admin.GetFromJsonAsync<UserResponse>("/api/v1/users/me", UsersApiClient.JsonOptions);

        me!.Email.Should().Be("admin@cloudgames.com");
        me.Name.Should().Be("Administrador FCG");
        me.Role.Should().Be(UserRole.Administrator);
    }
}
