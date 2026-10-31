using Fcg.Users.Domain._Shared.Exceptions;
using Fcg.Users.Domain.Users;

namespace Fcg.Users.UnitTests.Domain.Users;

public class PasswordTests
{
    [Theory]
    [InlineData("Abcd123!")]
    [InlineData("S3nh@Segura")]
    [InlineData("P@ssw0rd2024")]
    public void Create_DeveCriarSenha_QuandoAtendePolitica(string value)
    {
        Password.Create(value).Value.Should().Be(value);
    }

    [Theory]
    [InlineData("", "obrigatória")]
    [InlineData("Ab1!", "8 caracteres")]
    [InlineData("12345678!", "letra")]
    [InlineData("Password!", "número")]
    [InlineData("Password1", "caractere especial")]
    public void Create_DeveLancarExcecaoComMotivo_QuandoSenhaFraca(string value, string expectedReason)
    {
        var act = () => Password.Create(value);

        act.Should().Throw<BusinessException>().WithMessage($"*{expectedReason}*");
    }

    [Fact]
    public void ToString_NaoDeveExporValor_QuandoChamado()
    {
        Password.Create("Abcd123!").ToString().Should().NotContain("Abcd123!");
    }
}
