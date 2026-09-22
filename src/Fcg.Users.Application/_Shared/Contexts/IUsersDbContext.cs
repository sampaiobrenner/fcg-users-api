using Fcg.Users.Domain._Shared.Models;

namespace Fcg.Users.Application._Shared.Contexts;

public interface IUsersDbContext
{
    IQueryable<T> DataSet<T>() where T : PersistenceModelBase;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
