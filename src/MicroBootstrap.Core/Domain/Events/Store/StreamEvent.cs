using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;

namespace MicroBootstrap.Core.Domain.Events.Store;

public record StreamEvent
    (IDomainEvent Data, long StreamPosition, IStreamEventMetadata? Metadata = null) : Event, IStreamEvent;

public record StreamEvent<T>(T Data, long StreamPosition, IStreamEventMetadata? Metadata = null)
    : StreamEvent(Data, StreamPosition, Metadata), IStreamEvent<T>
    where T : IDomainEvent
{
    public new T Data => (T)base.Data;
}