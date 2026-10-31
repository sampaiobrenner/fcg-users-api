using Fcg.Users.Domain.Users;
using Fcg.Users.Infrastructure._Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.Infrastructure.Users;

internal sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await _context.Set<UserPersistenceModel>()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        return model is null ? null : new User(model);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        var model = await _context.Set<UserPersistenceModel>()
            .FirstOrDefaultAsync(user => user.Email == email.Value, cancellationToken);

        return model is null ? null : new User(model);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await _context.Set<UserPersistenceModel>().AddAsync(user.PersistenceModel, cancellationToken);
}
