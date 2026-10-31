using FluentValidation;

namespace Fcg.Users.Application.Users.ChangeUserRole;

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(command => command.Role).IsInEnum();
    }
}
