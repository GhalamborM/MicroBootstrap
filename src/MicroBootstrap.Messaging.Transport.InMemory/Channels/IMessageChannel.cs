using System.Threading.Channels;

namespace MicroBootstrap.Messaging.Transport.InMemory.Channels;

public interface IMessageChannel
{
    ChannelReader<IIntegrationEvent> Reader { get; }
    ChannelWriter<IIntegrationEvent> Writer { get; }
}
