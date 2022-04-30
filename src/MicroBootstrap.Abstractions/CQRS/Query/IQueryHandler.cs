using MediatR;

namespace MicroBootstrap.Abstractions.CQRS.Query;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}

// https://jimmybogard.com/mediatr-10-0-released/
public interface IStreamQueryHandler<in TQuery, TResponse> : IStreamRequestHandler<TQuery, TResponse>
    where TQuery : IStreamQuery<TResponse>
{
}
