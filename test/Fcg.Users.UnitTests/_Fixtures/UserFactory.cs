using Fcg.Users.Domain.Users;

namespace Fcg.Users.UnitTests._Fixtures;

internal static class UserFactory
{
    public const string PasswordHash = "stored-hash";

    public static User Create(string name = "Ada Lovelace", string email = "ada@fcg.dev", UserRole role = UserRole.User)
        => User.Create(name, Email.Create(email), PasswordHash, role);
}
