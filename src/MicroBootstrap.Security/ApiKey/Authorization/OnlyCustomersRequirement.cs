using Microsoft.AspNetCore.Authorization;

namespace MicroBootstrap.Security.ApiKey.Authorization;

public class OnlyCustomersRequirement : IAuthorizationRequirement
{
}
