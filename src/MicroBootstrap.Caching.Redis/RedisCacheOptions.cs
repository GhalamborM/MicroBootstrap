namespace MicroBootstrap.Caching.Redis;

public class RedisCacheOptions : CacheOptions
{
    public string? ConnectionString { get; set; }
    public int Db { get; set; } = -1;
    public object? AsyncState { get; set; } = null;
}
