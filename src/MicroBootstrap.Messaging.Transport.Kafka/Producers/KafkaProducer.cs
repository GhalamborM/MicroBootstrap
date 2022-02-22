namespace MicroBootstrap.Messaging.Transport.Kafka.Producers;

public class KafkaProducer : IEventBusPublisher
{
    private readonly KafkaProducerConfig _config;

    public KafkaProducer(IConfiguration configuration)
    {
        Guard.Against.Null(configuration, nameof(configuration));

        _config = configuration.GetKafkaProducerConfig();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        using (var p = new ProducerBuilder<string, string>(_config.ProducerConfig).Build())
        {
            await Task.Yield();

            var data = JsonConvert.SerializeObject(@event);

            // publish event to kafka topic taken from config
            await p.ProduceAsync(
                _config.Topic,
                new Message<string, string>
                {
                    // store event type name in message Key
                    Key = @event.GetType().Name,

                    // serialize event to message Value
                    Value = data
                },
                cancellationToken);
        }
    }
}
