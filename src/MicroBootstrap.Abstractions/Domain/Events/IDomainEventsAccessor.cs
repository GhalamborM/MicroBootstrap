using MicroBootstrap.Abstractions.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Domain.Events;

public interface IDomainEventsAccessor
{
    IReadOnlyList<IDomainEvent> UnCommittedDomainEvents { get; }
}
