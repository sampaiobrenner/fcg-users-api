using Fcg.Users.Domain._Shared.Models;

namespace Fcg.Users.Domain.Users;

public sealed class UserPersistenceModel : PersistenceModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}
