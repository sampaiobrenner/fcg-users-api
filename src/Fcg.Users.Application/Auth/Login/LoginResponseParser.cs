using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Domain.Users;

namespace Fcg.Users.Application.Auth.Login;

public sealed class LoginResponseParser : Parser<(User User, AccessToken Token), LoginResponse>
{
    protected override LoginResponse DoParse((User User, AccessToken Token) source)
        => new(
            source.Token.Token,
            source.Token.ExpiresAtUtc,
            source.User.Id(),
            source.User.PersistenceModel.Email,
            source.User.PersistenceModel.Role);
}
