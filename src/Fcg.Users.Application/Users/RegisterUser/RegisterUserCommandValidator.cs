using Fcg.Users.Application.Users._Shared;
using FluentValidation;

namespace Fcg.Users.Application.Users.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Name).ValidUserName();
        RuleFor(command => command.Email).ValidUserEmail();
        RuleFor(command => command.Password).ValidUserPassword();
    }
}
