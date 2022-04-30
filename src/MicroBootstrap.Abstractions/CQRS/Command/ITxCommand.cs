using MediatR;
using MicroBootstrap.Abstractions.Persistence;

namespace MicroBootstrap.Abstractions.CQRS.Command;

public interface ITxCommand : ITxCommand<Unit>
{
}

public interface ITxCommand<out T> : ICommand<T>, ITxRequest
    where T : notnull
{
}
