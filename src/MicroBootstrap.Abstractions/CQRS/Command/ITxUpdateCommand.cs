using MediatR;
using MicroBootstrap.Abstractions.Persistence;

namespace MicroBootstrap.Abstractions.CQRS.Command;

public interface ITxUpdateCommand<out TResponse> : IUpdateCommand<TResponse>, ITxRequest
    where TResponse : notnull
{
}

public interface ITxUpdateCommand : ITxUpdateCommand<Unit>
{
}
