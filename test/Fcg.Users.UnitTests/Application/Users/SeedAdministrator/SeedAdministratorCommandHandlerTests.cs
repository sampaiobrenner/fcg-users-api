using Fcg.Contracts.Events.V1;
using Fcg.Users.Application._Shared.Contexts;
using Fcg.Users.Application._Shared.Messaging;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Application.Users._Shared;
using Fcg.Users.Application.Users.SeedAdministrator;
using Fcg.Users.Domain.Users;
using Fcg.Users.UnitTests._Fixtures;
using Moq;

namespace Fcg.Users.UnitTests.Application.Users.SeedAdministrator;

public class SeedAdministratorCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUsersDbContext> _context = new();
    private readonly Mock<IIntegrationEventPublisher> _publisher = new();

    private SeedAdministratorCommandHandler CreateSut()
        => new(_users.Object, _hasher.Object, _context.Object, _publisher.Object, new UserCreatedEventParser());

    [Fact]
    public async Task Handle_DeveCriarAdministradorPadrao_QuandoNaoExiste()
    {
        _hasher.Setup(hasher => hasher.Hash("Admin@123")).Returns("admin-hash");

        var created = await CreateSut().Handle(new SeedAdministratorCommand("Administrador FCG", "admin@cloudgames.com", "Admin@123"), CancellationToken.None);

        created.Should().BeTrue();
        _users.Verify(repository => repository.AddAsync(
            It.Is<User>(user =>
                user.PersistenceModel.Name == "Administrador FCG" &&
                user.PersistenceModel.Email == "admin@cloudgames.com" &&
                user.PersistenceModel.PasswordHash == "admin-hash" &&
                user.IsAdministrator),
            It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(publisher => publisher.PublishAsync(
            It.Is<UserCreatedEvent>(integrationEvent =>
                integrationEvent.Email == "admin@cloudgames.com" && integrationEvent.Name == "Administrador FCG"),
            It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NaoDeveDuplicar_QuandoAdministradorJaExiste()
    {
        _users.Setup(repository => repository.GetByEmailAsync(Email.Create("admin@cloudgames.com"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserFactory.Create("Administrador FCG", "admin@cloudgames.com", UserRole.Administrator));

        var created = await CreateSut().Handle(new SeedAdministratorCommand("Administrador FCG", "admin@cloudgames.com", "Admin@123"), CancellationToken.None);

        created.Should().BeFalse();
        _users.Verify(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(publisher => publisher.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _context.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
