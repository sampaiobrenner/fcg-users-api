using Fcg.Users.Infrastructure._Shared.Security;

namespace Fcg.Users.UnitTests.Infrastructure._Shared.Security;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_DeveGerarHashBcryptComWorkFactor11_QuandoSenhaInformada()
    {
        var hash = _hasher.Hash("Abcd123!");

        hash.Should().NotBe("Abcd123!");
        hash.Should().StartWith("$2a$11$");
        hash.Length.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void Verify_DeveRetornarVerdadeiro_QuandoSenhaCorreta()
    {
        var hash = _hasher.Hash("Abcd123!");

        _hasher.Verify("Abcd123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_DeveRetornarFalso_QuandoSenhaIncorreta()
    {
        var hash = _hasher.Hash("Abcd123!");

        _hasher.Verify("Errada123!", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_DeveRetornarFalso_QuandoHashInvalido()
    {
        _hasher.Verify("Abcd123!", "hash-invalido").Should().BeFalse();
    }
}
