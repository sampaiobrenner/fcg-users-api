using Fcg.Users.Application.Users._Shared;
using FluentValidation;

namespace Fcg.Users.Application.Users.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Name).ValidUserName();
    }
}
