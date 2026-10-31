using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using MediatR;

namespace Fcg.Users.Application.Users.ChangeUserRole;

public sealed record ChangeUserRoleCommand(Guid Id, UserRole Role) : IRequest<UserResponse>;
