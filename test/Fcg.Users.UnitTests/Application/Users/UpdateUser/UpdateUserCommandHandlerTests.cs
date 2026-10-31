using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.UpdateUser;
using Fcg.Users.Domain.Users;
using Fcg.Users.UnitTests._Fixtures;
using Moq;

namespace Fcg.Users.UnitTests.Application.Users.UpdateUser;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUsersDbContext> _context = new();

    private UpdateUserCommandHandler CreateSut() => new(_users.Object, _context.Object, new UserResponseParser());

    [Fact]
    public async Task Handle_DeveRenomearESalvar_QuandoUsuarioExiste()
    {
        var user = UserFactory.Create();
        _users.Setup(repository => repository.GetByIdAsync(user.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var response = await CreateSut().Handle(new UpdateUserCommand(user.Id(), "Grace Hopper"), CancellationToken.None);

        response.Name.Should().Be("Grace Hopper");
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoUsuarioInexistente()
    {
        var act = () => CreateSut().Handle(new UpdateUserCommand(Guid.CreateVersion7(), "Grace"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Validator_DeveRetornarErro_QuandoNomeVazio()
    {
        new UpdateUserCommandValidator().Validate(new UpdateUserCommand(Guid.CreateVersion7(), "")).IsValid.Should().BeFalse();
    }
}
