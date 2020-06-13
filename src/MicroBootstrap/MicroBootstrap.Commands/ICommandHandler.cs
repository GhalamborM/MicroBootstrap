using MicroBootstrap.RabbitMq;
using MicroBootstrap.Messages;
using System.Threading.Tasks;

namespace MicroBootstrap.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, ICorrelationContext context);
    }
}