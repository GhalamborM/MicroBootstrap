namespace MicroBootstrap.Security.Jwt;

public class RolePolicy
{
    public string Name { get; set; }
    public IList<string> Roles { get; set; }
}
