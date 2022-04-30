using MediatR;
using MicroBootstrap.Abstractions.Persistence;

namespace MicroBootstrap.Abstractions.CQRS.Command;

public interface ITxCreateCommand<out TResponse> : ICommand<TResponse>, ITxRequest
    where TResponse : notnull
{
}

public interface ITxCreateCommand : ITxCreateCommand<Unit>
{
}
