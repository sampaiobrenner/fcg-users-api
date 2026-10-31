using Fcg.Contracts.Events.V1;
using MassTransit;

namespace Fcg.Users.IntegrationTests._Fixtures;

/// <summary>
/// Consumidor de teste ligado a exchange do evento. Com o bus outbox a entrega e um envio para
/// <c>fcg.user-created.v1</c>; consumir o evento prova que ele saiu do outbox e chegou ao broker no formato do contrato.
/// </summary>
internal sealed class UserCreatedEventProbeConsumer : IConsumer<UserCreatedEvent>
{
    public Task Consume(ConsumeContext<UserCreatedEvent> context) => Task.CompletedTask;
}
