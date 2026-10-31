using Fcg.Users.Domain._Shared.Exceptions;
using Fcg.Users.Domain.Users;

namespace Fcg.Users.UnitTests.Domain.Users;

public class EmailTests
{
    [Theory]
    [InlineData("user@cloudgames.com")]
    [InlineData("john.doe@example.co")]
    [InlineData("a_b+c@sub.domain.org")]
    public void Create_DeveCriarEmail_QuandoFormatoValido(string value)
    {
        Email.Create(value).Value.Should().Be(value.ToLowerInvariant());
    }

    [Fact]
    public void Create_DeveNormalizarParaMinusculasSemEspacos_QuandoInformadoComEspacos()
    {
        Email.Create("  User@CloudGames.COM  ").Value.Should().Be("user@cloudgames.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_DeveLancarExcecao_QuandoVazio(string? value)
    {
        var act = () => Email.Create(value);

        act.Should().Throw<BusinessException>().WithMessage("*obrigatório*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("no-at-sign.com")]
    [InlineData("missing@domain")]
    [InlineData("spaces in@email.com")]
    [InlineData("@no-local.com")]
    public void Create_DeveLancarExcecao_QuandoFormatoInvalido(string value)
    {
        var act = () => Email.Create(value);

        act.Should().Throw<BusinessException>().WithMessage("*formato inválido*");
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoExcedeTamanhoMaximo()
    {
        var value = $"{new string('a', Email.MaxLength)}@fcg.dev";

        var act = () => Email.Create(value);

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void TryCreate_DeveRetornarFalso_QuandoFormatoInvalido()
    {
        Email.TryCreate("invalid", out var email).Should().BeFalse();
        email.Should().BeNull();
    }

    [Fact]
    public void Equals_DeveSerIgual_QuandoMesmoValorNormalizado()
    {
        Email.Create("user@cloudgames.com").Should().Be(Email.Create("USER@cloudgames.com"));
        Email.Create("user@cloudgames.com").GetHashCode().Should().Be(Email.Create("USER@cloudgames.com").GetHashCode());
    }
}
