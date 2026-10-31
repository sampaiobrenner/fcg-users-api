using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Application.Properties;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using MediatR;

namespace Fcg.Users.Application.Users.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUserRepository _users;
    private readonly IUsersDbContext _context;
    private readonly IParser<UserPersistenceModel, UserResponse> _parser;

    public UpdateUserCommandHandler(IUserRepository users, IUsersDbContext context, IParser<UserPersistenceModel, UserResponse> parser)
    {
        _users = users;
        _context = context;
        _parser = parser;
    }

    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ApplicationResources.Usuario, request.Id);

        user.Rename(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        return _parser.Parse(user.PersistenceModel)!;
    }
}
