namespace Fcg.Users.Application._Shared.Security;

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
