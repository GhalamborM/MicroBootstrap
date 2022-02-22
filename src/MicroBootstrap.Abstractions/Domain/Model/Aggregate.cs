using System.Collections.Immutable;
using MicroBootstrap.Abstractions.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Domain.Exceptions;

namespace MicroBootstrap.Abstractions.Domain.Model;

public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
    [NonSerialized]
    private readonly List<IDomainEvent> _uncommittedDomainEvents = new();

    private bool _versionIncremented;

    public int Version { get; protected set; }

    /// <summary>
    /// Gets the aggregate root domain events.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _uncommittedDomainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (!_uncommittedDomainEvents.Any() && !_versionIncremented)
        {
            Version++;
            _versionIncremented = true;
        }

        _uncommittedDomainEvents.Add(domainEvent);
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

    public void IncrementVersion()
    {
        if (_versionIncremented)
        {
            return;
        }

        Version++;
        _versionIncremented = true;
    }

    public void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }

    public virtual void When(object @event)
    {
    }
}

public abstract class Aggregate<TIdentity, TId> : Aggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public abstract class Aggregate : Aggregate<AggregateId, long>, IAggregate
{
}
