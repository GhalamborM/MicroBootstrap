namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public sealed class PublisherChannelContext : IDisposable
{
    private readonly IPublisherChannelContextPool _publisherChannelContextPool;

    public PublisherChannelContext(
        IModel channel,
        QueueReferences queueReferences,
        IPublisherChannelContextPool publisherChannelContextPool)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        QueueReferences = queueReferences ?? throw new ArgumentNullException(nameof(queueReferences));
        _publisherChannelContextPool = publisherChannelContextPool ?? throw new ArgumentNullException(nameof(publisherChannelContextPool));
    }

    public IModel Channel { get; }
    public QueueReferences QueueReferences { get; }

    public void Dispose()
    {
        try
        {
            _publisherChannelContextPool.Return(this);
        }
        catch
        {
            //TODO
        }
    }
}
