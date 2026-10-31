using Fcg.Users.Domain.Users;

namespace Fcg.Users.Application._Shared.Security;

public interface IJwtTokenGenerator
{
    AccessToken Generate(User user);
}
