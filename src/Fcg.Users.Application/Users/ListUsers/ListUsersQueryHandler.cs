using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.Application.Users.ListUsers;

public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, IReadOnlyCollection<UserResponse>>
{
    private readonly IUsersDbContext _context;
    private readonly IParser<UserPersistenceModel, UserResponse> _parser;

    public ListUsersQueryHandler(IUsersDbContext context, IParser<UserPersistenceModel, UserResponse> parser)
    {
        _context = context;
        _parser = parser;
    }

    public async Task<IReadOnlyCollection<UserResponse>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.DataSet<UserPersistenceModel>()
            .OrderBy(model => model.Name)
            .ThenBy(model => model.Email)
            .ToListAsync(cancellationToken);

        return _parser.Parse(users);
    }
}
