namespace MicroBootstrap.Web.Module;

public interface IModuleDefinition
{
    IServiceCollection AddModuleServices(IServiceCollection services, IConfiguration configuration);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
    Task<WebApplication> ConfigureModule(WebApplication app);
}
