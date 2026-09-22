using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Domain._Shared.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.Infrastructure._Shared.Context;

public sealed class UsersDbContext : DbContext, IUsersDbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    public IQueryable<T> DataSet<T>() where T : PersistenceModelBase => Set<T>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
