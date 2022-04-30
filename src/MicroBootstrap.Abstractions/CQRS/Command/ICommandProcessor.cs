namespace MicroBootstrap.Abstractions.CQRS.Command;

public interface ICommandProcessor
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IInternalCommand internalCommandCommand, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IInternalCommand[] internalCommandCommands, CancellationToken cancellationToken = default);
}
