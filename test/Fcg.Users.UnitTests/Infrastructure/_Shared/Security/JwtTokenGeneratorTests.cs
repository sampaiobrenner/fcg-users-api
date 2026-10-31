using System.Text;
using Fcg.Contracts.Security;
using Fcg.Users.Domain.Users;
using Fcg.Users.Infrastructure._Shared.Security;
using Fcg.Users.UnitTests._Fixtures;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Fcg.Users.UnitTests.Infrastructure._Shared.Security;

public class JwtTokenGeneratorTests
{
    private const string Key = "fcg-dev-only-signing-key-change-me-0123456789";
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private static JwtTokenGenerator CreateSut(int? expirationMinutes = null)
    {
        var options = expirationMinutes is null
            ? new JwtOptions { Issuer = "fcg-users-api", Audience = "fcg", Key = Key }
            : new JwtOptions { Issuer = "fcg-users-api", Audience = "fcg", Key = Key, ExpirationMinutes = expirationMinutes.Value };

        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(provider => provider.GetUtcNow()).Returns(Now);

        return new JwtTokenGenerator(Options.Create(options), timeProvider.Object);
    }

    [Fact]
    public void Generate_DeveEmitirTokenComClaimsDoContrato_QuandoUsuarioInformado()
    {
        var user = UserFactory.Create(role: UserRole.Administrator);

        var accessToken = CreateSut().Generate(user);
        var token = new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token);

        token.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
        token.Issuer.Should().Be("fcg-users-api");
        token.Audiences.Should().ContainSingle().Which.Should().Be("fcg");
        token.GetClaim(FcgClaimTypes.UserId).Value.Should().Be(user.Id().ToString());
        token.GetClaim(FcgClaimTypes.Email).Value.Should().Be("ada@fcg.dev");
        token.GetClaim(FcgClaimTypes.Name).Value.Should().Be("Ada Lovelace");
        token.GetClaim(FcgClaimTypes.Role).Value.Should().Be(FcgRoles.Administrator);
        Guid.TryParse(token.GetClaim(JwtRegisteredClaimNames.Jti).Value, out _).Should().BeTrue();
        token.Claims.Should().NotContain(claim => claim.Type.StartsWith("http://", StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_DeveExpirarEm60Minutos_QuandoExpiracaoNaoConfigurada()
    {
        var accessToken = CreateSut().Generate(UserFactory.Create());
        var token = new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token);

        accessToken.ExpiresAtUtc.Should().Be(Now.UtcDateTime.AddMinutes(60));
        accessToken.ExpiresAtUtc.Kind.Should().Be(DateTimeKind.Utc);
        token.ValidTo.Should().Be(Now.UtcDateTime.AddMinutes(60));
    }

    [Fact]
    public void Generate_DeveRespeitarExpiracaoConfigurada_QuandoInformada()
    {
        var accessToken = CreateSut(expirationMinutes: 15).Generate(UserFactory.Create());

        accessToken.ExpiresAtUtc.Should().Be(Now.UtcDateTime.AddMinutes(15));
    }

    [Fact]
    public async Task Generate_DeveAssinarComChaveCompartilhada_QuandoValidadoComMesmosParametros()
    {
        var accessToken = CreateSut().Generate(UserFactory.Create());

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(accessToken.Token, new TokenValidationParameters
        {
            ValidIssuer = "fcg-users-api",
            ValidAudience = "fcg",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            ValidateLifetime = false
        });

        result.IsValid.Should().BeTrue();
    }
}
