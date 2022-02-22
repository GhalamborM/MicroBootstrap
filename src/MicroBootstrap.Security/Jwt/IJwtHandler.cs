using System.Security.Claims;

namespace MicroBootstrap.Security.Jwt;

public interface IJwtHandler
{
    public JsonWebToken GenerateJwtToken(
        string userName,
        string email,
        string userId,
        bool? isVerified = null,
        string fullName = null,
        string refreshToken = null,
        IList<Claim> usersClaims = null,
        IList<string> rolesClaims = null,
        IList<string> permissionsClaims = null);

    ClaimsPrincipal? ValidateToken(string token, TokenValidationParameters? tokenValidationParameters = null);
    JsonWebTokenPayload? GetTokenPayload(string accessToken);
}
