using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Application.Properties;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Users.Application.Users.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUsersDbContext _context;
    private readonly IParser<UserPersistenceModel, UserResponse> _parser;

    public GetUserByIdQueryHandler(IUsersDbContext context, IParser<UserPersistenceModel, UserResponse> parser)
    {
        _context = context;
        _parser = parser;
    }

    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.DataSet<UserPersistenceModel>()
            .FirstOrDefaultAsync(model => model.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(ApplicationResources.Usuario, request.Id);

        return _parser.Parse(user)!;
    }
}
