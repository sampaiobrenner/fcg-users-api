using Fcg.Contracts.Events.V1;
using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Messaging;
using Fcg.Users.Application._Shared.Parsers;
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
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IParser<User, UserCreatedEvent> _eventParser;

    public SeedAdministratorCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IUsersDbContext context,
        IIntegrationEventPublisher publisher,
        IParser<User, UserCreatedEvent> eventParser)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _context = context;
        _publisher = publisher;
        _eventParser = eventParser;
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

        // A NotificationsAPI tambem precisa conhecer o administrador; so publica na criacao.
        await _publisher.PublishAsync(_eventParser.Parse(administrator)!, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
