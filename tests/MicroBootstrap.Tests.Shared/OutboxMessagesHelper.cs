using Humanizer;
using MicroBootstrap.Abstractions.Messaging.Outbox;
using Newtonsoft.Json;

namespace BuildingBlocks.Tests.Integration.Helpers;

public class OutboxMessagesHelper
{
    private readonly IOutboxService _outboxService;
    private IEnumerable<OutboxMessage> _messages;

    public OutboxMessagesHelper(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    public async Task<List<OutboxMessage>> GetOutboxMessages()
    {
        _messages = await _outboxService.GetAllUnsentOutboxMessagesAsync();
        return _messages.ToList();
    }

    public T Deserialize<T>(OutboxMessage message) where T : class
    {
        return JsonConvert.DeserializeObject(message.Data, typeof(T)) as T;
    }

    public async Task<T> GetOutboxMessage<T>()
        where T : class
    {
        _messages ??= await GetOutboxMessages();
        var message = _messages.FirstOrDefault(x => x.Name == typeof(T).Name.Underscore());
        return Deserialize<T>(message);
    }
}