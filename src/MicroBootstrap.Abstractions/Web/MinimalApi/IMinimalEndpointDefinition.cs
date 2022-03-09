using Microsoft.AspNetCore.Routing;

namespace MicroBootstrap.Web.MinimalApi;

public interface IMinimalEndpointDefinition
{
    IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder);
}
