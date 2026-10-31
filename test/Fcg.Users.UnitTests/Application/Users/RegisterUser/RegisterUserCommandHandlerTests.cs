using Fcg.Contracts.Events.V1;
using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Exceptions;
using Fcg.Users.Application._Shared.Messaging;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.RegisterUser;
using Fcg.Users.Domain._Shared.Exceptions;
using Fcg.Users.Domain.Users;
using Fcg.Users.UnitTests._Fixtures;
using Moq;

namespace Fcg.Users.UnitTests.Application.Users.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUsersDbContext> _context = new();
    private readonly Mock<IIntegrationEventPublisher> _publisher = new();

    private RegisterUserCommandHandler CreateSut()
        => new(_users.Object, _hasher.Object, _context.Object, _publisher.Object, new UserCreatedEventParser(), new UserResponseParser());

    [Fact]
    public async Task Handle_DevePublicarUserCreatedEventAntesDoSaveChanges_QuandoUsuarioCriado()
    {
        var steps = new List<string>();
        UserCreatedEvent? published = null;
        _hasher.Setup(hasher => hasher.Hash(It.IsAny<string>())).Returns("hashed-secret");
        _publisher
            .Setup(publisher => publisher.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserCreatedEvent, CancellationToken>((integrationEvent, _) =>
            {
                published = integrationEvent;
                steps.Add("publish");
            })
            .Returns(Task.CompletedTask);
        _context
            .Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => steps.Add("save"))
            .ReturnsAsync(1);
        var before = DateTime.UtcNow;

        var response = await CreateSut().Handle(new RegisterUserCommand("Ada Lovelace", "ada@fcg.dev", "Str0ng!Pass"), CancellationToken.None);

        steps.Should().Equal("publish", "save");
        published.Should().NotBeNull();
        published!.UserId.Should().Be(response.Id);
        published.Name.Should().Be("Ada Lovelace");
        published.Email.Should().Be("ada@fcg.dev");
        published.EventId.Version.Should().Be(7);
        published.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
        published.OccurredAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Handle_DeveCriarUsuarioComHash_QuandoEmailNovo()
    {
        _hasher.Setup(hasher => hasher.Hash("Str0ng!Pass")).Returns("hashed-secret");

        var response = await CreateSut().Handle(new RegisterUserCommand("Ada Lovelace", " Ada@FCG.dev ", "Str0ng!Pass"), CancellationToken.None);

        response.Email.Should().Be("ada@fcg.dev");
        response.Name.Should().Be("Ada Lovelace");
        response.Role.Should().Be(UserRole.User);
        response.Active.Should().BeTrue();
        _users.Verify(repository => repository.AddAsync(
            It.Is<User>(user => user.PersistenceModel.PasswordHash == "hashed-secret" && user.Id() == response.Id),
            It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarConflito_QuandoEmailJaCadastrado()
    {
        _users.Setup(repository => repository.GetByEmailAsync(Email.Create("ada@fcg.dev"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserFactory.Create());

        var act = () => CreateSut().Handle(new RegisterUserCommand("Ada", "ADA@fcg.dev", "Str0ng!Pass"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*ada@fcg.dev*");
        _publisher.Verify(publisher => publisher.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _users.Verify(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecaoDeNegocio_QuandoSenhaFraca()
    {
        var act = () => CreateSut().Handle(new RegisterUserCommand("Ada", "ada@fcg.dev", "weak"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
        _hasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
    }
}
