using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fcg.Users.Application.Auth.Login;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.SeedAdministrator;

namespace Fcg.Users.IntegrationTests._Fixtures;

internal static class UsersApiClient
{
    public const string DefaultPassword = "Str0ng!Pass";

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static string NewEmail() => $"user-{Guid.NewGuid():N}@fcg.dev";

    public static async Task<UserResponse> RegisterAsync(this HttpClient client, string email, string name = "Ada Lovelace")
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { name, email, password = DefaultPassword });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserResponse>(JsonOptions))!;
    }

    public static async Task<LoginResponse> LoginAsync(this HttpClient client, string email, string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions))!;
    }

    public static async Task<HttpClient> CreateUserClientAsync(this UsersWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var email = NewEmail();
        await client.RegisterAsync(email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await client.LoginAsync(email)).AccessToken);
        return client;
    }

    public static async Task<HttpClient> CreateAdministratorClientAsync(this UsersWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var login = await client.LoginAsync(SeedAdministratorCommand.DefaultEmail, SeedAdministratorCommand.DefaultPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        return client;
    }
}
