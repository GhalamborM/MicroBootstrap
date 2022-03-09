namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store.Projections;

public interface IHaveAggregateStateProjection
{
    void When(object @event);
    void When(object @event, long version);
}