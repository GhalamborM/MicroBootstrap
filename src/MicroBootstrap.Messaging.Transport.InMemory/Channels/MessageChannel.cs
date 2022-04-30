using System.Threading.Channels;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;

namespace MicroBootstrap.Messaging.Transport.InMemory.Channels;

public class MessageChannel : IMessageChannel
{
    private readonly Channel<IIntegrationEvent> _messages = Channel.CreateUnbounded<IIntegrationEvent>();
    public ChannelReader<IIntegrationEvent> Reader => _messages.Reader;
    public ChannelWriter<IIntegrationEvent> Writer => _messages.Writer;
}
