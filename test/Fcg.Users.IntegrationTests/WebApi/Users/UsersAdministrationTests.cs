using System.Net;
using System.Net.Http.Json;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using Fcg.Users.IntegrationTests._Fixtures;

namespace Fcg.Users.IntegrationTests.WebApi.Users;

[Collection(UsersApiCollection.Name)]
public class UsersAdministrationTests
{
    private readonly UsersWebApplicationFactory _factory;

    public UsersAdministrationTests(UsersWebApplicationFactory factory) => _factory = factory;

    public static TheoryData<string, string> AdminEndpoints => new()
    {
        { "GET", "/api/v1/users" },
        { "GET", $"/api/v1/users/{Guid.CreateVersion7()}" },
        { "PUT", $"/api/v1/users/{Guid.CreateVersion7()}" },
        { "PUT", $"/api/v1/users/{Guid.CreateVersion7()}/role" },
        { "DELETE", $"/api/v1/users/{Guid.CreateVersion7()}" }
    };

    [Theory]
    [MemberData(nameof(AdminEndpoints))]
    public async Task EndpointsDeAdministracao_DevemRetornarForbidden_QuandoPapelUser(string method, string url)
    {
        var client = await _factory.CreateUserClientAsync();
        using var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = method == "PUT" ? JsonContent.Create(new { name = "X", role = "Administrator" }) : null
        };

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListUsers_DeveRetornarOk_QuandoAdministrador()
    {
        var admin = await _factory.CreateAdministratorClientAsync();

        var response = await admin.GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>(UsersApiClient.JsonOptions);
        users.Should().Contain(user => user.Email == "admin@cloudgames.com");
    }

    [Fact]
    public async Task ListUsers_DeveRetornarNaoAutorizado_QuandoSemToken()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_DeveRetornarNotFound_QuandoInexistente()
    {
        var admin = await _factory.CreateAdministratorClientAsync();

        var response = await admin.GetAsync($"/api/v1/users/{Guid.CreateVersion7()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Administracao_DeveAtualizarPromoverEDesativar_QuandoAdministrador()
    {
        var admin = await _factory.CreateAdministratorClientAsync();
        var email = UsersApiClient.NewEmail();
        var user = await admin.RegisterAsync(email);

        var byId = await admin.GetFromJsonAsync<UserResponse>($"/api/v1/users/{user.Id}", UsersApiClient.JsonOptions);
        byId.Should().BeEquivalentTo(user);

        var update = await admin.PutAsJsonAsync($"/api/v1/users/{user.Id}", new { name = "Grace Hopper" });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<UserResponse>(UsersApiClient.JsonOptions))!.Name.Should().Be("Grace Hopper");

        var role = await admin.PutAsJsonAsync($"/api/v1/users/{user.Id}/role", new { role = "Administrator" });
        role.StatusCode.Should().Be(HttpStatusCode.OK);
        (await role.Content.ReadFromJsonAsync<UserResponse>(UsersApiClient.JsonOptions))!.Role.Should().Be(UserRole.Administrator);

        var delete = await admin.DeleteAsync($"/api/v1/users/{user.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deactivated = await admin.GetFromJsonAsync<UserResponse>($"/api/v1/users/{user.Id}", UsersApiClient.JsonOptions);
        deactivated!.Active.Should().BeFalse();

        var login = await admin.PostAsJsonAsync("/api/v1/auth/login", new { email, password = UsersApiClient.DefaultPassword });
        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUser_DeveRetornarBadRequest_QuandoNomeVazio()
    {
        var admin = await _factory.CreateAdministratorClientAsync();
        var user = await admin.RegisterAsync(UsersApiClient.NewEmail());

        var response = await admin.PutAsJsonAsync($"/api/v1/users/{user.Id}", new { name = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("PUT", "")]
    [InlineData("PUT", "/role")]
    [InlineData("DELETE", "")]
    public async Task Escrita_DeveRetornarNotFound_QuandoUsuarioInexistente(string method, string suffix)
    {
        var admin = await _factory.CreateAdministratorClientAsync();
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/api/v1/users/{Guid.CreateVersion7()}{suffix}")
        {
            Content = method == "PUT" ? JsonContent.Create(new { name = "Grace", role = "User" }) : null
        };

        var response = await admin.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
