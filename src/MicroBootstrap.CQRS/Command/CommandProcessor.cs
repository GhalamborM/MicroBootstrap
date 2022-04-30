using MediatR;
using MicroBootstrap.Abstractions.CQRS.Command;
using MicroBootstrap.Abstractions.Scheduler;

namespace MicroBootstrap.CQRS.Command;

public class CommandProcessor : ICommandProcessor
{
    private readonly IMediator _mediator;
    private readonly ICommandScheduler _commandScheduler;

    public CommandProcessor(
        IMediator mediator,
        ICommandScheduler commandScheduler
    )
    {
        _mediator = mediator;
        _commandScheduler = commandScheduler;
    }

    public Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        return _mediator.Send(command, cancellationToken);
    }

    public Task ScheduleAsync(IInternalCommand internalCommandCommand, CancellationToken cancellationToken = default)
    {
        return _commandScheduler.ScheduleAsync(internalCommandCommand, cancellationToken);
    }

    public Task ScheduleAsync(IInternalCommand[] internalCommandCommands, CancellationToken cancellationToken = default)
    {
        return _commandScheduler.ScheduleAsync(internalCommandCommands, cancellationToken);
    }
}
