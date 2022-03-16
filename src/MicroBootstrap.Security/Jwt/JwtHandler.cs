using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ardalis.GuardClauses;
using MicroBootstrap.Core.Extensions.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MicroBootstrap.Security.Jwt;

public sealed class JwtHandler : IJwtHandler
{
    private static readonly ISet<string> _defaultClaims = new HashSet<string>(StringComparer.Ordinal)
    {
        JwtRegisteredClaimNames.Sub,
        JwtRegisteredClaimNames.UniqueName,
        JwtRegisteredClaimNames.Jti,
        JwtRegisteredClaimNames.Iat,
        ClaimTypes.Role
    };

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly ILogger<JwtHandler> _logger;
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly SigningCredentials _signingCredentials;

    public JwtHandler(
        JwtOptions options,
        TokenValidationParameters tokenValidationParameters,
        ILogger<JwtHandler> logger)
    {
        var issuerSigningKey = tokenValidationParameters.IssuerSigningKey;

        _options = Guard.Against.Null(options, nameof(options));
        Guard.Against.Null(issuerSigningKey, nameof(issuerSigningKey));
        Guard.Against.Null(_options.Algorithm, nameof(_options.Algorithm));

        _signingCredentials =
            new SigningCredentials(issuerSigningKey, _options.Algorithm);

        _tokenValidationParameters = tokenValidationParameters;
        _logger = logger;
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/415
        _jwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
        _jwtSecurityTokenHandler.OutboundClaimTypeMap.Clear();
    }

    public JsonWebToken GenerateJwtToken(
        string userName,
        string email,
        string userId,
        bool? isVerified = null,
        string? fullName = null,
        string? refreshToken = null,
        IList<Claim>? usersClaims = null,
        IList<string>? rolesClaims = null,
        IList<string>? permissionsClaims = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User ID claim (subject) cannot be empty.", nameof(userName));

        var now = DateTime.Now;
        var ipAddress = IpHelper.GetIpAddress();

        // https://leastprivilege.com/2017/11/15/missing-claims-in-the-asp-net-core-2-openid-connect-handler/
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/a301921ff5904b2fe084c38e41c969f4b2166bcb/src/System.IdentityModel.Tokens.Jwt/ClaimTypeMapping.cs#L45-L125
        // https://stackoverflow.com/a/50012477/581476
        var jwtClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, userId),
            new(JwtRegisteredClaimNames.Name, fullName ?? ""),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Sid, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.GivenName, fullName ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
            new("rt", refreshToken ?? ""),
            new("ip", ipAddress)
        };

        if (rolesClaims?.Any() is true)
        {
            foreach (var role in rolesClaims)
                jwtClaims.Add(new Claim(ClaimTypes.Role, role.ToLower(CultureInfo.InvariantCulture)));
        }

        if (!string.IsNullOrWhiteSpace(_options.Audience))
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Aud, _options.Audience));

        if (permissionsClaims?.Any() is true)
        {
            foreach (var permissionsClaim in permissionsClaims)
            {
                jwtClaims.Add(new Claim(
                    CustomClaimTypes.Permission,
                    permissionsClaim.ToLower(CultureInfo.InvariantCulture)));
            }
        }

        if (usersClaims?.Any() is true)
            jwtClaims = jwtClaims.Union(usersClaims).ToList();

        var expire = now.AddMinutes(_options.ExpiryMinutes == 0 ? 120 : _options.ExpiryMinutes);

        var jwt = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            notBefore: now,
            claims: jwtClaims,
            expires: expire,
            signingCredentials: _signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);


        return new JsonWebToken
        {
            IsVerified = isVerified,
            AccessToken = token,
            Expires = expire,
            UserId = userId,
            Email = email,
            Roles = rolesClaims?.ToList() ?? Enumerable.Empty<string>().ToList(),
            Permissions = permissionsClaims?.ToList() ?? Enumerable.Empty<string>().ToList()
        };
    }

    public ClaimsPrincipal? ValidateToken(string token, TokenValidationParameters? tokenValidationParameters = null)
    {
        try
        {
            var principal =
                _jwtSecurityTokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters ?? _tokenValidationParameters,
                    out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        catch (System.Exception e)
        {
            _logger.LogError("Token validation failed: {Message}", e.Message);
            return null;
        }
    }

    public JsonWebTokenPayload? GetTokenPayload(string accessToken)
    {
        _jwtSecurityTokenHandler.ValidateToken(accessToken, _tokenValidationParameters, out var validatedSecurityToken);
        if (!(validatedSecurityToken is JwtSecurityToken jwt))
            return null;

        return new JsonWebTokenPayload
        {
            Subject = jwt.Subject,
            Role = jwt.Claims
                .SingleOrDefault(x => string.Equals(x.Type, ClaimTypes.Role, StringComparison.Ordinal))?.Value!,
            Expires = jwt.ValidTo.ToUnixTimeMilliseconds(),
            Claims = jwt.Claims.Where(x => !_defaultClaims.Contains(x.Type))
                .GroupBy(c => c.Type)
                .ToDictionary(k => k.Key, v => v.Select(c => c.Value))
        };
    }
}
