using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events.Store.InMemory;

namespace MicroBootstrap.Core.Tests.Fixtures;

public class IntegrationFixture
{
    public IntegrationFixture()
    {
        EventStore = new InMemoryEventStore();
        EventStoreRepository = new EventStoreRepository(EventStore);
    }

    public IEventStore EventStore { get; }
    public IEventStoreRepository EventStoreRepository { get; }

    public static IntegrationFixture Instance { get; } = new();
}