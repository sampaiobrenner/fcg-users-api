namespace Fcg.Users.Infrastructure._Shared.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumKeyLength = 32;
    public const int DefaultExpirationMinutes = 60;

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = DefaultExpirationMinutes;
}
