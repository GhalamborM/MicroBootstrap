using System.Security.Claims;

namespace MicroBootstrap.Security.Jwt;

public class ClaimPolicy
{
    public string Name { get; set; }
    public IList<Claim> Claims { get; set; }
}
