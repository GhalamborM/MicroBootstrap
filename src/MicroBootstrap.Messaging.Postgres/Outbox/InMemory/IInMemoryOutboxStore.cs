using MicroBootstrap.Abstractions.Messaging.Outbox;

namespace MicroBootstrap.Messaging.Outbox.InMemory;

public interface IInMemoryOutboxStore
{
    public IList<OutboxMessage> Events { get; }
}
