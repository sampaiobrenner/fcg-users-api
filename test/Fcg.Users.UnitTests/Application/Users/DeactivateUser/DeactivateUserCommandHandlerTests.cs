using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application.Users.DeactivateUser;
using Fcg.Users.Domain.Users;
using Fcg.Users.UnitTests._Fixtures;
using Moq;

namespace Fcg.Users.UnitTests.Application.Users.DeactivateUser;

public class DeactivateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUsersDbContext> _context = new();

    private DeactivateUserCommandHandler CreateSut() => new(_users.Object, _context.Object);

    [Fact]
    public async Task Handle_DeveDesativarSemApagar_QuandoUsuarioExiste()
    {
        var user = UserFactory.Create();
        _users.Setup(repository => repository.GetByIdAsync(user.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await CreateSut().Handle(new DeactivateUserCommand(user.Id()), CancellationToken.None);

        user.PersistenceModel.Active.Should().BeFalse();
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoUsuarioInexistente()
    {
        var act = () => CreateSut().Handle(new DeactivateUserCommand(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
