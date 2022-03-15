using Ardalis.GuardClauses;
using Marten;
using Marten.Services;
using MicroBootstrap.Abstractions.Core.Domain.Events;

namespace MicroBootstrap.Persistence.Marten;

public class MartenUnitOfWork : IMartenUnitOfWork
{
    private readonly IDocumentStore _documentStore;
    private readonly IEventProcessor _eventProcessor;
    private readonly IDomainEventsAccessor _domainEventsAccessor;
    private IDocumentSession _session;


    public MartenUnitOfWork(
        IDocumentStore documentStore,
        IEventProcessor eventProcessor,
        IDomainEventsAccessor domainEventsAccessor)
    {
        _documentStore = documentStore;
        _eventProcessor = Guard.Against.Null(eventProcessor, nameof(eventProcessor));
        _domainEventsAccessor = Guard.Against.Null(domainEventsAccessor, nameof(domainEventsAccessor));
        _session = _documentStore.OpenSession(new SessionOptions { });
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _session = await _documentStore.OpenSessionAsync(new SessionOptions() { }, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var uncommittedEvents = _domainEventsAccessor.UnCommittedDomainEvents.ToArray();

        await _eventProcessor.PublishAsync(uncommittedEvents, cancellationToken);

        await _session.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _session.Dispose();
        GC.SuppressFinalize(this);
    }
}