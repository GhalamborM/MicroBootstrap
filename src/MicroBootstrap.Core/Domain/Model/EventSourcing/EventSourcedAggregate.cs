using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using MicroBootstrap.Abstractions.Core.Domain;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;
using MicroBootstrap.Abstractions.Domain.Exceptions;
using MicroBootstrap.Core.Extensions.Utils.Reflections;

namespace MicroBootstrap.Abstractions.Domain.Model.EventSourcing;

public class EventSourcedAggregate<TId> : Entity<TId>, IEventSourcedAggregate<TId>
{
    [NonSerialized]
    private readonly ConcurrentQueue<IDomainEvent> _uncommittedDomainEvents = new();

    // -1: No Stream
    public const long NewAggregateVersion = -1;

    /// <inheritdoc />
    public long OriginalVersion { get; private set; } = NewAggregateVersion;

    /// <inheritdoc />
    public long CurrentVersion { get; private set; } = NewAggregateVersion;

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _uncommittedDomainEvents.ToImmutableList();

    /// <summary>
    /// Applies a new event to the aggregate state, adds the event to the list of pending changes,
    /// and increases the `CurrentVersion` property and `OriginalState` will be unchanged.
    /// </summary>
    /// <typeparam name="TDomainEvent">Type of domain event.</typeparam>
    /// <param name="domainEvent"></param>
    protected virtual void ApplyEvent<TDomainEvent>(TDomainEvent domainEvent)
        where TDomainEvent : IDomainEvent
    {
        AddDomainEvent(domainEvent);
        When(domainEvent);
        CurrentVersion++;
    }

    /// <inheritdoc />
    public void When(object @event)
    {
        if (GetType().HasAggregateApplyMethod(@event.GetType()))
        {
            this.InvokeMethod("Apply", @event);
        }
        else
        {
            throw new AggregateException($"Can't find 'Apply' method for domain event: '{@event.GetType().Name}'");
        }
    }

    /// <inheritdoc />
    public void Fold(object @event)
    {
        When(@event);
        OriginalVersion++;
        CurrentVersion++;
    }

    /// <inheritdoc />
    public void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            Fold(domainEvent);
        }
    }

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (!_uncommittedDomainEvents.Any(x => Equals(x.EventId, domainEvent.EventId)))
        {
            IDomainEvent eventWithAggregate = domainEvent.WithAggregate(Id, CurrentVersion + 1);
            _uncommittedDomainEvents.Enqueue(eventWithAggregate);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> FlushUncommittedEvents()
    {
        var events = _uncommittedDomainEvents.ToImmutableList();

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