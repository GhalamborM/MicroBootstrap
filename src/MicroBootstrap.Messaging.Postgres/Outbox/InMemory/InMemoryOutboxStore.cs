using MicroBootstrap.Abstractions.Messaging.Outbox;

namespace MicroBootstrap.Messaging.Outbox.InMemory;

public class InMemoryOutboxStore : IInMemoryOutboxStore
{
    public IList<OutboxMessage> Events { get; } = new List<OutboxMessage>();
}
