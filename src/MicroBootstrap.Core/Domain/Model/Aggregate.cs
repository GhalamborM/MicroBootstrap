using System.Collections.Concurrent;
using System.Collections.Immutable;
using MicroBootstrap.Abstractions.Core.Domain;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Model;
using MicroBootstrap.Abstractions.Domain.Exceptions;

namespace MicroBootstrap.Abstractions.Domain.Model;

public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
    public const long NewAggregateVersion = -1;

    [NonSerialized] private readonly ConcurrentQueue<IDomainEvent> _uncommittedDomainEvents = new();

    /// <summary>
    /// Gets or sets current version of our aggregate.
    /// It should increase for each state transition performed.
    /// </summary>
    public long Version { get; private set; } = NewAggregateVersion;

    /// <summary>
    /// Get the list of pending changes domain events to be applied to the aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _uncommittedDomainEvents.ToImmutableList();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)))
        {
            _uncommittedDomainEvents.Enqueue(domainEvent);
        }
    }

    public IReadOnlyList<IDomainEvent> FlushUncommittedEvents()
    {
        var events = _uncommittedDomainEvents.ToImmutableList();

        var i = 0;
        foreach (var domainEvent in events)
        {
            i++;
            domainEvent.WithAggregate(Id, Version + i);
        }

        Version += events.Count;

        _uncommittedDomainEvents.Clear();

        return events;
    }

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents()
    {
        return _uncommittedDomainEvents.ToImmutableList();
    }

    public void ClearUncommittedEvents()
    {
        _uncommittedDomainEvents.Clear();
    }

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