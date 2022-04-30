namespace MicroBootstrap.Abstractions.CQRS.Command;

public abstract record TxInternalCommand : InternalCommand, ITxInternalCommand;
