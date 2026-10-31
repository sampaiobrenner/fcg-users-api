using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Domain.Users;
using MediatR;

namespace Fcg.Users.Application.Users.SeedAdministrator;

/// <summary>Cria o administrador padrao somente se ele ainda nao existir (idempotente).</summary>
public sealed class SeedAdministratorCommandHandler : IRequestHandler<SeedAdministratorCommand, bool>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUsersDbContext _context;

    public SeedAdministratorCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, IUsersDbContext context)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _context = context;
    }

    public async Task<bool> Handle(SeedAdministratorCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(SeedAdministratorCommand.DefaultEmail);

        if (await _users.GetByEmailAsync(email, cancellationToken) is not null)
            return false;

        var administrator = User.Create(
            SeedAdministratorCommand.DefaultName,
            email,
            _passwordHasher.Hash(SeedAdministratorCommand.DefaultPassword),
            UserRole.Administrator);

        await _users.AddAsync(administrator, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
