using System.Text;
using Fcg.Contracts.Security;
using Fcg.Users.Application._Shared.Security;
using Fcg.Users.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Users.Infrastructure._Shared.Security;

/// <summary>
/// Emite o JWT (HS256) aceito por todos os servicos da plataforma: claims curtas do contrato
/// (<c>sub</c>, <c>email</c>, <c>name</c>, <c>role</c>) mais <c>jti</c>.
/// </summary>
internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public AccessToken Generate(User user)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(_options.ExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [FcgClaimTypes.UserId] = user.Id().ToString(),
                [FcgClaimTypes.Email] = user.PersistenceModel.Email,
                [FcgClaimTypes.Name] = user.PersistenceModel.Name,
                [FcgClaimTypes.Role] = user.PersistenceModel.Role.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            }
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }
}
