using MicroBootstrap.Abstractions.Messaging.Outbox;

namespace MicroBootstrap.Messaging.InMemory.Outbox;

public interface IInMemoryOutboxStore
{
    public IList<OutboxMessage> Events { get; }
}
