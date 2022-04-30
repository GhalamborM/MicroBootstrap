using MicroBootstrap.Abstractions.Messaging.Outbox;

namespace MicroBootstrap.Messaging.InMemory.Outbox;

public class InMemoryOutboxStore : IInMemoryOutboxStore
{
    public IList<OutboxMessage> Events { get; } = new List<OutboxMessage>();
}
