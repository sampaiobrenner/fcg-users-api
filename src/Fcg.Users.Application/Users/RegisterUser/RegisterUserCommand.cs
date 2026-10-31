using Fcg.Users.Application.Users._Shared;
using MediatR;

namespace Fcg.Users.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(string Name, string Email, string Password) : IRequest<UserResponse>;
