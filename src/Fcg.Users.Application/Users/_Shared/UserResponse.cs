using Fcg.Users.Domain.Users;

namespace Fcg.Users.Application.Users._Shared;

public sealed record UserResponse(Guid Id, string Name, string Email, UserRole Role, bool Active, DateTime CreatedAt);
