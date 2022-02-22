using System.Security.Claims;

namespace MicroBootstrap.Security.Jwt;

internal sealed class DisabledAuthenticationPolicyEvaluator : IPolicyEvaluator
{
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(),
            new AuthenticationProperties(), JwtBearerDefaults.AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy,
        AuthenticateResult authenticationResult, HttpContext context, object resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
}
