using Fcg.Users.Domain._Shared.Exceptions;
using Fcg.Users.Domain._Shared.Models;
using Fcg.Users.Domain.Properties;

namespace Fcg.Users.Domain.Users;

/// <summary>
/// Raiz do agregado de usuario: identidade e acesso (cadastro, papel e situacao).
/// A biblioteca de jogos pertence a CatalogAPI.
/// </summary>
public sealed class User : DomainModelBase<UserPersistenceModel>, IAggregateRoot
{
    public const int NameMaxLength = 120;

    public User(UserPersistenceModel persistenceModel) : base(persistenceModel)
    {
    }

    public bool IsAdministrator => PersistenceModel.Role == UserRole.Administrator;

    public static User Create(string name, Email email, string passwordHash, UserRole role = UserRole.User)
    {
        if (email is null)
            throw new BusinessException(DomainResources.EmailObrigatorio);

        var user = new User(new UserPersistenceModel
        {
            Email = email.Value,
            Active = true,
            CreatedAt = DateTime.UtcNow
        });

        user.Rename(name);
        user.ChangePasswordHash(passwordHash);
        user.ChangeRole(role);

        return user;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException(DomainResources.NomeObrigatorio);

        var trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
            throw new BusinessException(string.Format(DomainResources.NomeTamanhoMaximo, NameMaxLength));

        PersistenceModel.Name = trimmed;
    }

    public void ChangeRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
            throw new BusinessException(DomainResources.PapelInvalido);

        PersistenceModel.Role = role;
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessException(DomainResources.HashDaSenhaObrigatorio);

        PersistenceModel.PasswordHash = passwordHash;
    }

    public void Activate() => PersistenceModel.Active = true;

    public void Deactivate() => PersistenceModel.Active = false;
}
