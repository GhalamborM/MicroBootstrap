namespace MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

public interface IDomainEventContext
{
    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
}
