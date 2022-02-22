namespace MicroBootstrap.Abstractions.CQRS.Command;

public interface IInternalCommand : ICommand
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string CommandType { get; }
}
