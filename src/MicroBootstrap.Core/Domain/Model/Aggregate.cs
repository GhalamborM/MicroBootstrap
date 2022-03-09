using System.Collections.Concurrent;
using System.Collections.Immutable;
using MicroBootstrap.Abstractions.Core.Domain;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model;
using MicroBootstrap.Abstractions.Domain.Exceptions;
using MicroBootstrap.Core.Extensions.Utils.Reflections;

namespace MicroBootstrap.Abstractions.Domain.Model;

public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>, IHaveEventSourcingAggregate
{
    public const long NewAggregateVersion = -1;

    [NonSerialized] private readonly ConcurrentQueue<IDomainEvent> _uncommittedDomainEvents = new();

    /// <summary>
    /// Gets or sets current version of our aggregate.
    /// It should increase for each state transition performed.
    /// </summary>
    public long Version { get; protected set; } = NewAggregateVersion;

    /// <summary>
    /// Get the list of pending changes domain events to be applied to the aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _uncommittedDomainEvents.ToImmutableList();

    public string StreamName => $"{GetType().Name}-{Id}";

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)))
        {
            IDomainEvent eventWithAggregate = domainEvent.WithAggregate(Id, Version);
            _uncommittedDomainEvents.Enqueue(eventWithAggregate);
        }
    }

    public IReadOnlyList<IDomainEvent> FlushUncommittedEvents()
    {
        var events = _uncommittedDomainEvents.ToImmutableList();

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

    /// <summary>
    /// Applies a new event to the aggregate state, adds the event to the list of pending changes,
    /// and increases the current version.
    /// </summary>
    /// <typeparam name="TDomainEvent">Type of domain event.</typeparam>
    /// <param name="domainEvent"></param>
    protected void ApplyEvent<TDomainEvent>(TDomainEvent domainEvent)
        where TDomainEvent : IDomainEvent
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)))
        {
            AddDomainEvent(domainEvent);
            When(domainEvent);
        }
    }

    public virtual void When<TDomainEvent>(TDomainEvent domainEvent)
        where TDomainEvent : IDomainEvent
    {
        When(domainEvent, Version++);
    }

    public virtual void When<TDomainEvent>(TDomainEvent domainEvent, long version)
        where TDomainEvent : IDomainEvent
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)) &&
            GetType().HasAggregateApplyMethod<TDomainEvent>())
        {
            ((dynamic)this).Apply((dynamic)domainEvent);
            Version = version;
        }
        else
        {
            throw new AggregateException($"Can't find 'Apply' method for domain event: '{domainEvent.GetType().Name}'");
        }
    }

    public void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            When(domainEvent);
        }
    }

    public void When(object @event)
    {
        When(@event, Version++);
    }

    public void When(object @event, long version)
    {
        if (GetType().HasAggregateApplyMethod(@event.GetType()))
        {
            ((dynamic)this).Apply((dynamic)@event);
            Version = version;
        }
        else
        {
            throw new AggregateException($"Can't find 'Apply' method for domain event: '{@event.GetType().Name}'");
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