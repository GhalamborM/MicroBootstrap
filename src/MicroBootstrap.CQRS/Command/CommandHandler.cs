using MediatR;
using MicroBootstrap.Abstractions.CQRS.Command;

namespace MicroBootstrap.CQRS.Command;

public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    protected abstract Task<Unit> HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

    public Task<Unit> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}

public abstract class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    protected abstract Task<TResponse> HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

    public Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}
