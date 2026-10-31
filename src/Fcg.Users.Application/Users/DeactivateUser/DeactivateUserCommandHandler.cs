using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application.Properties;
using Fcg.Users.Domain.Users;
using MediatR;

namespace Fcg.Users.Application.Users.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IUsersDbContext _context;

    public DeactivateUserCommandHandler(IUserRepository users, IUsersDbContext context)
    {
        _users = users;
        _context = context;
    }

    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ApplicationResources.Usuario, request.Id);

        // Desativacao logica: o registro e mantido e o login passa a ser recusado.
        user.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
