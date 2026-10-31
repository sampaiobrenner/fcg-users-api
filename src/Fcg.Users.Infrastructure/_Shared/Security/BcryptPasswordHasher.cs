using Fcg.Users.Application._Shared.Security;

namespace Fcg.Users.Infrastructure._Shared.Security;

internal sealed class BcryptPasswordHasher : IPasswordHasher
{
    public const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash corrompido ou em formato desconhecido: trata como credencial invalida.
            return false;
        }
    }
}
