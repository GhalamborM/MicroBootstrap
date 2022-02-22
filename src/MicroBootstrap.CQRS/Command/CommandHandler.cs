namespace MicroBootstrap.CQRS.Command;

public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly IOutboxService _outboxService;

    protected CommandHandler(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    protected abstract Task<Unit> HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

    public Task<Unit> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}

public abstract class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly IOutboxService _outboxService;

    protected CommandHandler(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    protected abstract Task<TResponse> HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

    public Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}
