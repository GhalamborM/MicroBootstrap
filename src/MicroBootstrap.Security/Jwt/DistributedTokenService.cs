namespace MicroBootstrap.Security.Jwt;

public class DistributedTokenService : IAccessTokenService
{
    private readonly TimeSpan _expires;
    private readonly ICacheManager _cacheManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DistributedTokenService(
        ICacheManager cacheManager,
        IHttpContextAccessor httpContextAccessor,
        IOptions<JwtOptions> jwtOptions)
    {
        _cacheManager = Guard.Against.Null(cacheManager, nameof(cacheManager));
        _httpContextAccessor = httpContextAccessor;
        _expires = TimeSpan.FromMinutes(jwtOptions.Value?.ExpiryMinutes ?? 120);
    }

    public Task<bool> IsCurrentActiveToken()
    {
        return IsActiveAsync(GetCurrent());
    }

    public Task DeactivateCurrentAsync()
    {
        return DeactivateAsync(GetCurrent());
    }

    public async Task<bool> IsActiveAsync(string token)
    {
        return string.IsNullOrWhiteSpace(await _cacheManager.GetAsync<string>(GetKey(token)));
    }

    public Task DeactivateAsync(string token)
    {
        return _cacheManager.SetAsync(GetKey(token), "revoked", _expires.Seconds);
    }

    private string GetCurrent()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Get<string>("authorization");

        return authorizationHeader is null || authorizationHeader == StringValues.Empty
            ? string.Empty
            : authorizationHeader.Split(' ').Last();
    }

    private static string GetKey(string token)
    {
        return $"blacklisted-tokens:{token}";
    }
}
