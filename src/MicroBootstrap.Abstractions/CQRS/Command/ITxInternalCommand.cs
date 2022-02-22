using MicroBootstrap.Abstractions.Persistence;

namespace MicroBootstrap.Abstractions.CQRS.Command;

public interface ITxInternalCommand : IInternalCommand, ITxRequest
{
}
