using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Application.Auth.Login;
using Fcg.Users.Domain.Users;
using Fcg.Users.UnitTests._Fixtures;
using Moq;

namespace Fcg.Users.UnitTests.Application.Auth.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new();

    private LoginCommandHandler CreateSut()
        => new(_users.Object, _hasher.Object, _tokenGenerator.Object, new LoginResponseParser());

    private User SetupExistingUser(bool active = true)
    {
        var user = UserFactory.Create();
        if (!active)
            user.Deactivate();

        _users.Setup(repository => repository.GetByEmailAsync(Email.Create("ada@fcg.dev"), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return user;
    }

    [Fact]
    public async Task Handle_DeveRetornarToken_QuandoCredenciaisValidas()
    {
        var user = SetupExistingUser();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        _hasher.Setup(hasher => hasher.Verify("Str0ng!Pass", UserFactory.PasswordHash)).Returns(true);
        _tokenGenerator.Setup(generator => generator.Generate(user)).Returns(new AccessToken("jwt-token", expiresAt));

        var response = await CreateSut().Handle(new LoginCommand("ADA@fcg.dev", "Str0ng!Pass"), CancellationToken.None);

        response.Should().Be(new LoginResponse("jwt-token", expiresAt, user.Id(), "ada@fcg.dev", UserRole.User));
    }

    [Fact]
    public async Task Handle_DeveLancarNaoAutorizadoGenerico_QuandoSenhaErrada()
    {
        SetupExistingUser();
        _hasher.Setup(hasher => hasher.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var act = () => CreateSut().Handle(new LoginCommand("ada@fcg.dev", "errada"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Credenciais inválidas.");
        _tokenGenerator.Verify(generator => generator.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveLancarNaoAutorizadoGenerico_QuandoEmailDesconhecido()
    {
        var act = () => CreateSut().Handle(new LoginCommand("ghost@fcg.dev", "whatever"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Credenciais inválidas.");
    }

    [Fact]
    public async Task Handle_DeveLancarNaoAutorizadoGenerico_QuandoEmailEmFormatoInvalido()
    {
        var act = () => CreateSut().Handle(new LoginCommand("ada@dominio", "whatever"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Credenciais inválidas.");
    }

    [Fact]
    public async Task Handle_DeveLancarNaoAutorizado_QuandoUsuarioInativo()
    {
        SetupExistingUser(active: false);
        _hasher.Setup(hasher => hasher.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var act = () => CreateSut().Handle(new LoginCommand("ada@fcg.dev", "Str0ng!Pass"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Usuário inativo.");
        _tokenGenerator.Verify(generator => generator.Generate(It.IsAny<User>()), Times.Never);
    }
}
