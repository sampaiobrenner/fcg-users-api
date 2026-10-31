using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Application.Properties;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using MediatR;

namespace Fcg.Users.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserResponse>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUsersDbContext _context;
    private readonly IParser<UserPersistenceModel, UserResponse> _parser;

    public RegisterUserCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IUsersDbContext context,
        IParser<UserPersistenceModel, UserResponse> parser)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _context = context;
        _parser = parser;
    }

    public async Task<UserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await _users.GetByEmailAsync(email, cancellationToken) is not null)
            throw new ConflictException(string.Format(ApplicationResources.EmailJaCadastrado, email.Value));

        // A politica de senha e invariante de dominio; apenas o hash e persistido.
        var password = Password.Create(request.Password);
        var user = User.Create(request.Name, email, _passwordHasher.Hash(password.Value));

        await _users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _parser.Parse(user.PersistenceModel)!;
    }
}
