using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.GetMyProfile;
using Fcg.Users.UnitTests._Fixtures;
using Moq;

namespace Fcg.Users.UnitTests.Application.Users.GetMyProfile;

public class GetMyProfileQueryHandlerTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();

    [Fact]
    public async Task Handle_DeveRetornarPerfil_QuandoUsuarioDoTokenExiste()
    {
        await using var context = InMemoryUsersDbContext.Create();
        var user = UserFactory.Create();
        context.Add(user.PersistenceModel);
        await context.SaveChangesAsync();
        _currentUser.Setup(current => current.UserId).Returns(user.Id());

        var response = await new GetMyProfileQueryHandler(context, _currentUser.Object, new UserResponseParser())
            .Handle(new GetMyProfileQuery(), CancellationToken.None);

        response.Id.Should().Be(user.Id());
        response.Email.Should().Be("ada@fcg.dev");
    }

    [Fact]
    public async Task Handle_DeveLancarNaoAutorizado_QuandoTokenSemUsuario()
    {
        await using var context = InMemoryUsersDbContext.Create();

        var act = () => new GetMyProfileQueryHandler(context, _currentUser.Object, new UserResponseParser())
            .Handle(new GetMyProfileQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoUsuarioDoTokenNaoExiste()
    {
        await using var context = InMemoryUsersDbContext.Create();
        _currentUser.Setup(current => current.UserId).Returns(Guid.CreateVersion7());

        var act = () => new GetMyProfileQueryHandler(context, _currentUser.Object, new UserResponseParser())
            .Handle(new GetMyProfileQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
