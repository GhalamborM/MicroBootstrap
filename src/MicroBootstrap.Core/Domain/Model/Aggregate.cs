using System.Collections.Concurrent;
using System.Collections.Immutable;
using MicroBootstrap.Abstractions.Core.Domain;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Model;
using MicroBootstrap.Core.Domain.Exceptions;

namespace MicroBootstrap.Core.Domain.Model;

public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
    [NonSerialized]
    private readonly ConcurrentQueue<IDomainEvent> _uncommittedDomainEvents = new();

    // -1: No Stream
    public const long NewAggregateVersion = -1;

    /// We should update original version with current version when we have no optimistic concurrency issues during save
    /// <inheritdoc />
    public long OriginalVersion { get; private set; } = NewAggregateVersion;

    /// <inheritdoc />
    public long CurrentVersion { get; private set; } = NewAggregateVersion;

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _uncommittedDomainEvents.ToImmutableList();

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)))
        {
            _uncommittedDomainEvents.Enqueue(domainEvent);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> FlushUncommittedEvents()
    {
        var events = _uncommittedDomainEvents.ToImmutableList();

        foreach (var domainEvent in events)
        {
            CurrentVersion++;
            domainEvent.WithAggregate(Id, CurrentVersion);
        }

        _uncommittedDomainEvents.Clear();

        return events;
    }

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> GetUncommittedEvents()
    {
        return _uncommittedDomainEvents.ToImmutableList();
    }

    /// <inheritdoc />
    public void ClearUncommittedEvents()
    {
        _uncommittedDomainEvents.Clear();
    }

    /// <inheritdoc />
    public void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}

public abstract class Aggregate<TIdentity, TId> : Aggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public abstract class Aggregate : Aggregate<AggregateId, long>, IAggregate
{
}