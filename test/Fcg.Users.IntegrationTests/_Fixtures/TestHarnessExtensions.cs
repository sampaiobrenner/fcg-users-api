using Fcg.Contracts.Events.V1;
using MassTransit.Testing;

namespace Fcg.Users.IntegrationTests._Fixtures;

internal static class TestHarnessExtensions
{
    public const string UserCreatedExchange = "fcg.user-created.v1";

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Aguarda o <see cref="UserCreatedEvent"/> do e-mail chegar ao consumidor de teste. Como a publicacao passa pelo
    /// outbox, a entrega e assincrona (servico de entrega do outbox) e nao basta a deteccao de inatividade do harness.
    /// </summary>
    public static async Task<IReceivedMessage<UserCreatedEvent>?> WaitForUserCreatedAsync(this ITestHarness harness, string email, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);

        do
        {
            var received = harness.Consumed
                .Select<UserCreatedEvent>(message => message.Context.Message.Email == email)
                .FirstOrDefault();

            if (received is not null)
                return received;

            await Task.Delay(PollInterval);
        }
        while (DateTime.UtcNow < deadline);

        return null;
    }

    public static int CountUserCreated(this ITestHarness harness, string email)
        => harness.Consumed.Select<UserCreatedEvent>(message => message.Context.Message.Email == email).Count();
}
