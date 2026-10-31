using Fcg.Users.Application.Users._Shared;
using MediatR;

namespace Fcg.Users.Application.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserResponse>;
