using System.Security.Claims;

namespace MicroBootstrap.Core.Extensions.Utils;

public static class ClaimsPrincipalExtensions
{
    public static string GetClaimValue(this ClaimsPrincipal principal, string type)
    {
        return principal.FindFirst(type)!.Value;
    }
}
