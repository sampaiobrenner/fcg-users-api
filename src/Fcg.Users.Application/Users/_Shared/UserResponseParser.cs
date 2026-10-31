using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Domain.Users;

namespace Fcg.Users.Application.Users._Shared;

public sealed class UserResponseParser : Parser<UserPersistenceModel, UserResponse>
{
    protected override UserResponse DoParse(UserPersistenceModel source)
        => new(source.Id, source.Name, source.Email, source.Role, source.Active, source.CreatedAt);
}
