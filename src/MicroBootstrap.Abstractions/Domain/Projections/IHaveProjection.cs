namespace MicroBootstrap.Abstractions.Domain.Projections;

public interface IHaveProjection
{
    void When(object @event);
}
