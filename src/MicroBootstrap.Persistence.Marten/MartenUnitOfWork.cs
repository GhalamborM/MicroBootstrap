namespace MicroBootstrap.Persistence.Marten;

public class MartenUnitOfWork : IMartenUnitOfWork
{
    private readonly IDocumentSession _documentSession;
    private readonly IEventProcessor _eventProcessor;
    private readonly IDomainEventsAccessor _domainEventsAccessor;

    public MartenUnitOfWork(
        IDocumentSession documentSession,
        IEventProcessor eventProcessor,
        IDomainEventsAccessor domainEventsAccessor)
    {
        _documentSession = documentSession;
        _eventProcessor = eventProcessor;
        _domainEventsAccessor = domainEventsAccessor;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var uncommittedEvents = _domainEventsAccessor.UnCommittedDomainEvents.ToArray();

        await _documentSession.SaveChangesAsync(cancellationToken);
        await _eventProcessor.PublishAsync(uncommittedEvents, cancellationToken);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
