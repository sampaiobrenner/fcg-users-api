using Fcg.Users.Domain._Shared.Exceptions;
using Fcg.Users.Domain.Users;
using Fcg.Users.UnitTests._Fixtures;

namespace Fcg.Users.UnitTests.Domain.Users;

public class UserTests
{
    [Fact]
    public void Create_DeveCriarUsuarioAtivo_QuandoDadosValidos()
    {
        var before = DateTime.UtcNow;

        var user = User.Create("  Ada Lovelace  ", Email.Create("Ada@FCG.dev"), "hash");

        user.Id().Should().NotBeEmpty();
        user.Id().Version.Should().Be(7);
        user.PersistenceModel.Name.Should().Be("Ada Lovelace");
        user.PersistenceModel.Email.Should().Be("ada@fcg.dev");
        user.PersistenceModel.PasswordHash.Should().Be("hash");
        user.PersistenceModel.Role.Should().Be(UserRole.User);
        user.PersistenceModel.Active.Should().BeTrue();
        user.PersistenceModel.CreatedAt.Should().BeOnOrAfter(before);
        user.PersistenceModel.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        (user.PersistenceModel.CreatedAt.Ticks % TimeSpan.TicksPerMicrosecond).Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_DeveLancarExcecao_QuandoNomeVazio(string name)
    {
        var act = () => User.Create(name, Email.Create("a@b.com"), "hash");

        act.Should().Throw<BusinessException>().WithMessage("*nome*");
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoNomeExcedeTamanhoMaximo()
    {
        var act = () => User.Create(new string('a', User.NameMaxLength + 1), Email.Create("a@b.com"), "hash");

        act.Should().Throw<BusinessException>().WithMessage("*120*");
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoHashVazio()
    {
        var act = () => User.Create("Ada", Email.Create("a@b.com"), " ");

        act.Should().Throw<BusinessException>().WithMessage("*hash*");
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoEmailNulo()
    {
        var act = () => User.Create("Ada", null!, "hash");

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void IsAdministrator_DeveRefletirPapel_QuandoConsultado()
    {
        UserFactory.Create(role: UserRole.Administrator).IsAdministrator.Should().BeTrue();
        UserFactory.Create(role: UserRole.User).IsAdministrator.Should().BeFalse();
    }

    [Fact]
    public void Rename_DeveAlterarNome_QuandoValido()
    {
        var user = UserFactory.Create();

        user.Rename(" Grace Hopper ");

        user.PersistenceModel.Name.Should().Be("Grace Hopper");
    }

    [Fact]
    public void ChangeRole_DeveAlterarPapel_QuandoValido()
    {
        var user = UserFactory.Create();

        user.ChangeRole(UserRole.Administrator);

        user.IsAdministrator.Should().BeTrue();
    }

    [Fact]
    public void ChangeRole_DeveLancarExcecao_QuandoPapelInexistente()
    {
        var act = () => UserFactory.Create().ChangeRole((UserRole)99);

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void ChangePasswordHash_DeveAlterarHash_QuandoValido()
    {
        var user = UserFactory.Create();

        user.ChangePasswordHash("novo-hash");

        user.PersistenceModel.PasswordHash.Should().Be("novo-hash");
    }

    [Fact]
    public void DeactivateEActivate_DevemAlternarSituacao_QuandoChamados()
    {
        var user = UserFactory.Create();

        user.Deactivate();
        user.PersistenceModel.Active.Should().BeFalse();

        user.Activate();
        user.PersistenceModel.Active.Should().BeTrue();
    }
}
