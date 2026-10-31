using Fcg.Users.Application.Users.RegisterUser;

namespace Fcg.Users.UnitTests.Application.Users.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_DeveSerValido_QuandoDadosCorretos()
    {
        _validator.Validate(new RegisterUserCommand("Ada", "ada@fcg.dev", "Str0ng!Pass")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "ada@fcg.dev", "Str0ng!Pass", "Name")]
    [InlineData("Ada", "", "Str0ng!Pass", "Email")]
    [InlineData("Ada", "invalido", "Str0ng!Pass", "Email")]
    [InlineData("Ada", "ada@dominio", "Str0ng!Pass", "Email")]
    [InlineData("Ada", "ada@fcg.dev", "Ab1!", "Password")]
    [InlineData("Ada", "ada@fcg.dev", "12345678!", "Password")]
    [InlineData("Ada", "ada@fcg.dev", "Password!", "Password")]
    [InlineData("Ada", "ada@fcg.dev", "Password1", "Password")]
    public void Validate_DeveRetornarErroNoCampo_QuandoDadoInvalido(string name, string email, string password, string property)
    {
        var result = _validator.Validate(new RegisterUserCommand(name, email, password));

        result.Errors.Should().Contain(error => error.PropertyName == property);
    }

    [Fact]
    public void Validate_DeveRetornarErro_QuandoNomeExcede120Caracteres()
    {
        var result = _validator.Validate(new RegisterUserCommand(new string('a', 121), "ada@fcg.dev", "Str0ng!Pass"));

        result.Errors.Should().Contain(error => error.PropertyName == "Name");
    }
}
