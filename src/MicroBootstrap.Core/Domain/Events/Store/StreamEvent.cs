using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Events;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public record StreamEvent(IDomainEvent Data, IStreamEventMetadata Metadata) : Event, IStreamEvent;

public record StreamEvent<T>(T Data, IStreamEventMetadata Metadata) : Event, IStreamEvent<T>
    where T : IDomainEvent;