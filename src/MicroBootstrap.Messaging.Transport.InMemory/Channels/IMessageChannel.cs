using System.Threading.Channels;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;

namespace MicroBootstrap.Messaging.Transport.InMemory.Channels;

public interface IMessageChannel
{
    ChannelReader<IIntegrationEvent> Reader { get; }
    ChannelWriter<IIntegrationEvent> Writer { get; }
}
