using Fcg.Users.Application.Users._Shared;
using MediatR;

namespace Fcg.Users.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(Guid Id, string Name) : IRequest<UserResponse>;
