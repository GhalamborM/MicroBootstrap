using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IStreamEvent : IStreamEvent<IDomainEvent>
{
}

public interface IStreamEvent<T> : IEvent
    where T : IDomainEvent
{
    public T Data { get; init; }
    public IStreamEventMetadata Metadata { get; init; }
}