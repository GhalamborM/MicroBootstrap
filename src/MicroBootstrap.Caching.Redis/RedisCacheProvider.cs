using Ardalis.GuardClauses;
using MicroBootstrap.Abstractions.Caching;
using MicroBootstrap.Abstractions.Messaging.Serialization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MicroBootstrap.Caching.Redis;

public class RedisCacheProvider : ICacheProvider
{
    private readonly IMessageSerializer _messageSerializer;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly RedisCacheOptions _cacheOptions;

    private IDatabase Database => _connectionMultiplexer.GetDatabase(_cacheOptions.Db, _cacheOptions.AsyncState);

    public RedisCacheProvider(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RedisCacheOptions> cacheOptions,
        IMessageSerializer messageSerializer)
    {
        _messageSerializer = Guard.Against.Null(messageSerializer, nameof(messageSerializer));
        _connectionMultiplexer = Guard.Against.Null(connectionMultiplexer, nameof(connectionMultiplexer));
        _cacheOptions = Guard.Against.Null(cacheOptions.Value, nameof(cacheOptions));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        var value = await Database.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : _messageSerializer.Deserialize<T>(value);
    }

    public void Set(string key, object data, int? cacheTime = null)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));
        Guard.Against.Null(data, nameof(data));

        var json = _messageSerializer.Serialize(data);
        var time = TimeSpan.FromSeconds(cacheTime ?? _cacheOptions.DefaultCacheTime);

        Database.StringSet(key, json, time);
    }

    public Task<bool> IsSetAsync(string key)
    {
        return Database.KeyExistsAsync(key);
    }

    public Task RemoveAsync(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        return Database.KeyDeleteAsync(key);
    }

    public T? Get<T>(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        var value = Database.StringGet(key);
        return value.IsNullOrEmpty ? default : _messageSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync(string key, object data, int? cacheTime = null)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));
        Guard.Against.Null(data, nameof(data));

        var json = _messageSerializer.Serialize(data);
        await Database.StringSetAsync(key, json, TimeSpan.FromSeconds(cacheTime ?? _cacheOptions.DefaultCacheTime));
    }

    public bool IsSet(string key)
    {
        return Database.KeyExists(key);
    }

    public void Remove(string key)
    {
        Guard.Against.NullOrEmpty(key, nameof(key));

        Database.KeyDelete(key);
    }
}
