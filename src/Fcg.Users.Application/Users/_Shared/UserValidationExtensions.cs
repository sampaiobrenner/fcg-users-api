using Fcg.Users.Application.Properties;
using Fcg.Users.Domain.Users;
using FluentValidation;

namespace Fcg.Users.Application.Users._Shared;

/// <summary>Regras de validacao de entrada compartilhadas pelos casos de uso de usuario.</summary>
public static class UserValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidUserName<T>(this IRuleBuilder<T, string> rule)
        => rule.NotEmpty().MaximumLength(User.NameMaxLength);

    public static IRuleBuilderOptions<T, string> ValidUserEmail<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty()
            .MaximumLength(Email.MaxLength)
            .EmailAddress()
            .Must(Email.IsValid).WithMessage(ApplicationResources.EmailFormatoInvalido);

    public static IRuleBuilderOptions<T, string> ValidUserPassword<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty()
            .MinimumLength(Password.MinLength).WithMessage(string.Format(ApplicationResources.SenhaTamanhoMinimo, Password.MinLength))
            .Matches("[A-Za-z]").WithMessage(ApplicationResources.SenhaSemLetra)
            .Matches("[0-9]").WithMessage(ApplicationResources.SenhaSemNumero)
            .Matches("[^A-Za-z0-9]").WithMessage(ApplicationResources.SenhaSemCaractereEspecial);
}
