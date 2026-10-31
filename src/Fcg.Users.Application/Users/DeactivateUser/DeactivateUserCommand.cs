using MediatR;

namespace Fcg.Users.Application.Users.DeactivateUser;

public sealed record DeactivateUserCommand(Guid Id) : IRequest;
