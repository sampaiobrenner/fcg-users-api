namespace Fcg.Users.Domain.Users;

/// <summary>
/// Niveis de acesso da plataforma. O nome do membro e o valor da claim <c>role</c>
/// (igual a <c>Fcg.Contracts.Security.FcgRoles</c>).
/// </summary>
public enum UserRole
{
    /// <summary>Acesso a plataforma e a biblioteca de jogos.</summary>
    User = 1,

    /// <summary>Cadastra jogos, administra usuarios e cria promocoes.</summary>
    Administrator = 2
}
