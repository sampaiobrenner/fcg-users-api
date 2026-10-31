using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Fcg.Users.Domain._Shared.Exceptions;
using Fcg.Users.Domain.Properties;

namespace Fcg.Users.Domain.Users;

/// <summary>E-mail valido e normalizado (minusculas, sem espacos nas bordas).</summary>
public sealed partial class Email
{
    public const int MaxLength = 320;

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException(DomainResources.EmailObrigatorio);

        if (!TryCreate(value, out var email))
            throw new BusinessException(string.Format(DomainResources.EmailFormatoInvalido, value));

        return email;
    }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out Email? email)
    {
        email = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength || !EmailRegex().IsMatch(normalized))
            return false;

        email = new Email(normalized);
        return true;
    }

    public static bool IsValid(string? value) => TryCreate(value, out _);

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is Email other && other.Value == Value;

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
