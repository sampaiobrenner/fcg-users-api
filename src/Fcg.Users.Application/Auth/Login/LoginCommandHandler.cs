using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Application.Properties;
using Fcg.Users.Domain.Users;
using MediatR;

namespace Fcg.Users.Application.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IParser<(User User, AccessToken Token), LoginResponse> _parser;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IParser<(User User, AccessToken Token), LoginResponse> parser)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _parser = parser;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Mensagem generica: nao revela se o e-mail existe.
        if (!Email.TryCreate(request.Email, out var email))
            throw new UnauthorizedException(ApplicationResources.CredenciaisInvalidas);

        var user = await _users.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PersistenceModel.PasswordHash))
            throw new UnauthorizedException(ApplicationResources.CredenciaisInvalidas);

        if (!user.PersistenceModel.Active)
            throw new UnauthorizedException(ApplicationResources.UsuarioInativo);

        return _parser.Parse((user, _tokenGenerator.Generate(user)))!;
    }
}
