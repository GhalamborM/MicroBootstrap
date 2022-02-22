namespace MicroBootstrap.Abstractions.Domain.Events.Internal;

public interface IDomainEventContext
{
    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
}
