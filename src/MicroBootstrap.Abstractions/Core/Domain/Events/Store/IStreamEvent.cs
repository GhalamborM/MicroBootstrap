using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IStreamEvent : IEvent
{
    public IDomainEvent Data { get; }

    /// <summary>
    /// Gets EventNumber or EventSequenceNumber
    /// </summary>
    public long StreamPosition { get; }

    public IStreamEventMetadata? Metadata { get; }
}

public interface IStreamEvent<out T> : IStreamEvent
    where T : IDomainEvent
{
    public new T Data { get; }
}