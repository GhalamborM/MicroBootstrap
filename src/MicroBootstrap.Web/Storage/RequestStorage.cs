using MicroBootstrap.Abstractions.Caching;
using MicroBootstrap.Abstractions.Web.Storage;

namespace MicroBootstrap.Web.Storage;

internal class RequestStorage : IRequestStorage
{
    private readonly ICacheManager _cacheManager;


    public RequestStorage(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public void Set<T>(string key, T value)
        where T : notnull =>
        _cacheManager.Set(key, value, TimeSpan.FromSeconds(5).Seconds);

    public T? Get<T>(string key)
        => _cacheManager.Get<T>(key);
}
