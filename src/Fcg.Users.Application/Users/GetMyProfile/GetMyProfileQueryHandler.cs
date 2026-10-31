using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Application.Properties;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.Application.Users.GetMyProfile;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserResponse>
{
    private readonly IUsersDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IParser<UserPersistenceModel, UserResponse> _parser;

    public GetMyProfileQueryHandler(IUsersDbContext context, ICurrentUser currentUser, IParser<UserPersistenceModel, UserResponse> parser)
    {
        _context = context;
        _currentUser = currentUser;
        _parser = parser;
    }

    public async Task<UserResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException(ApplicationResources.UsuarioNaoAutenticado);

        var user = await _context.DataSet<UserPersistenceModel>()
            .FirstOrDefaultAsync(model => model.Id == userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationResources.Usuario, userId);

        return _parser.Parse(user)!;
    }
}
