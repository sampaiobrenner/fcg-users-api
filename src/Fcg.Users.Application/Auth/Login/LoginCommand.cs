using MediatR;

namespace Fcg.Users.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
