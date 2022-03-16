using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events;

namespace MicroBootstrap.Core.Persistence.EventStore;

public record StreamEvent
    (IDomainEvent Data,  IStreamEventMetadata? Metadata = null) : Event, IStreamEvent;

public record StreamEvent<T>(T Data,  IStreamEventMetadata? Metadata = null)
    : StreamEvent(Data, Metadata), IStreamEvent<T>
    where T : IDomainEvent
{
    public new T Data => (T)base.Data;
}