using Fcg.Users.Application.Users._Shared;
using MediatR;

namespace Fcg.Users.Application.Users.ListUsers;

public sealed record ListUsersQuery : IRequest<IReadOnlyCollection<UserResponse>>;
