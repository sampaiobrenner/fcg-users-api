using Fcg.Users.Domain.Users;

namespace Fcg.Users.Application.Auth.Login;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, Guid UserId, string Email, UserRole Role);
