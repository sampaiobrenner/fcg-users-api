using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.ChangeUserRole;
using Fcg.Users.Domain.Users;
using Fcg.Users.UnitTests._Fixtures;
using Moq;

namespace Fcg.Users.UnitTests.Application.Users.ChangeUserRole;

public class ChangeUserRoleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUsersDbContext> _context = new();

    private ChangeUserRoleCommandHandler CreateSut() => new(_users.Object, _context.Object, new UserResponseParser());

    [Fact]
    public async Task Handle_DeveAlterarPapelESalvar_QuandoUsuarioExiste()
    {
        var user = UserFactory.Create();
        _users.Setup(repository => repository.GetByIdAsync(user.Id(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var response = await CreateSut().Handle(new ChangeUserRoleCommand(user.Id(), UserRole.Administrator), CancellationToken.None);

        response.Role.Should().Be(UserRole.Administrator);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoEncontrado_QuandoUsuarioInexistente()
    {
        var act = () => CreateSut().Handle(new ChangeUserRoleCommand(Guid.CreateVersion7(), UserRole.Administrator), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public void Validator_DeveRetornarErro_QuandoPapelInexistente()
    {
        new ChangeUserRoleCommandValidator().Validate(new ChangeUserRoleCommand(Guid.CreateVersion7(), (UserRole)99)).IsValid.Should().BeFalse();
    }
}
