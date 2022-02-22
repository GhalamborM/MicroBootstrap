namespace MicroBootstrap.Persistence.Marten;

public class MartenEventStore : IEventStore
{
    private readonly IDocumentSession _documentSession;

    public MartenEventStore(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public Task Append(
        Guid streamId,
        int? version,
        CancellationToken cancellationToken = default,
        params IEvent[] events)
    {
        if (version.HasValue)
            _documentSession.Events.Append(streamId, version, events.Cast<object>().ToArray());
        else
            _documentSession.Events.Append(streamId, events.Cast<object>().ToArray());
        return Task.CompletedTask;
    }

    public Task Append(Guid streamId, CancellationToken cancellationToken, params IEvent[] events)
    {
        return Append(streamId, null, cancellationToken, events);
    }

    public Task<TEntity?> Aggregate<TEntity>(
        Guid streamId,
        CancellationToken cancellationToken = default,
        int version = 0,
        DateTime? timestamp = null)
        where TEntity : class, new()
    {
        return _documentSession.Events.AggregateStreamAsync<TEntity>(
            streamId,
            version,
            timestamp,
            token: cancellationToken);
    }

    public async Task<IReadOnlyList<IEvent>> Query(
        Guid? streamId = null,
        CancellationToken cancellationToken = default,
        int? fromVersion = null,
        DateTime? fromTimestamp = null)
    {
        var events = await Filter(streamId, fromVersion, fromTimestamp)
            .ToListAsync(cancellationToken);

        return events
            .Select(ev => ev.Data)
            .OfType<IEvent>()
            .ToList();
    }

    public async Task<IReadOnlyList<TEvent>> Query<TEvent>(
        Guid? streamId = null,
        CancellationToken cancellationToken = default,
        int? fromVersion = null,
        DateTime? fromTimestamp = null)
        where TEvent : class, IEvent
    {
        var events = await Filter(streamId, fromVersion, fromTimestamp)
            .ToListAsync(cancellationToken);

        return events
            .Select(ev => ev.Data)
            .OfType<TEvent>()
            .ToImmutableList();
    }

    private IQueryable<global::Marten.Events.IEvent> Filter(Guid? streamId, int? version, DateTime? timestamp)
    {
        var query = _documentSession.Events.QueryAllRawEvents().AsQueryable();

        if (streamId.HasValue)
            query = query.Where(ev => ev.StreamId == streamId);

        if (version.HasValue)
            query = query.Where(ev => ev.Version >= version);

        if (timestamp.HasValue)
            query = query.Where(ev => ev.Timestamp >= timestamp);

        return query;
    }

    public Task SaveChanges(CancellationToken token = default)
    {
        return _documentSession.SaveChangesAsync(token);
    }
}
