using Fcg.Users.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.UnitTests._Fixtures;

internal static class InMemoryUsersDbContext
{
    public static UsersDbContext Create()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase($"fcg-users-{Guid.NewGuid()}")
            .Options;

        return new UsersDbContext(options);
    }
}
