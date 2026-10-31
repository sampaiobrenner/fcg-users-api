using Fcg.Contracts.Events.V1;
using Fcg.Users.Application._Shared.Parsers;
using Fcg.Users.Domain.Users;

namespace Fcg.Users.Application.Users._Shared;

/// <summary>Monta o <see cref="UserCreatedEvent"/> do contrato: EventId novo (v7) a cada publicacao e data em UTC.</summary>
public sealed class UserCreatedEventParser : Parser<User, UserCreatedEvent>
{
    protected override UserCreatedEvent DoParse(User source)
        => new(
            EventId: Guid.CreateVersion7(),
            OccurredAt: DateTime.UtcNow,
            UserId: source.Id(),
            Name: source.PersistenceModel.Name,
            Email: source.PersistenceModel.Email);
}
