namespace Fcg.Users.WebApi._Shared.Database;

/// <summary>Administrador criado no startup. A senha vem do Secret (<c>Seed__Administrator__Password</c>).</summary>
public sealed class AdministratorSeedOptions
{
    public const string SectionName = "Seed:Administrator";

    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}
