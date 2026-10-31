using Fcg.Users.Domain._Shared.Exceptions;
using Fcg.Users.Domain.Properties;

namespace Fcg.Users.Domain.Users;

/// <summary>
/// Senha em texto puro validada pela politica de seguranca (minimo de 8 caracteres com letra,
/// numero e caractere especial). Objeto transitorio: nunca e persistido, apenas o hash.
/// </summary>
public sealed class Password
{
    public const int MinLength = 8;

    private Password(string value) => Value = value;

    public string Value { get; }

    public static Password Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException(DomainResources.SenhaObrigatoria);

        if (value.Length < MinLength)
            throw new BusinessException(string.Format(DomainResources.SenhaTamanhoMinimo, MinLength));

        if (!value.Any(char.IsLetter))
            throw new BusinessException(DomainResources.SenhaSemLetra);

        if (!value.Any(char.IsDigit))
            throw new BusinessException(DomainResources.SenhaSemNumero);

        if (value.All(char.IsLetterOrDigit))
            throw new BusinessException(DomainResources.SenhaSemCaractereEspecial);

        return new Password(value);
    }

    public override string ToString() => "********";
}
