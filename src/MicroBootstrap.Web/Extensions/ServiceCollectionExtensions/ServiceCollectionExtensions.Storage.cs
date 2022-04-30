using MicroBootstrap.Abstractions.Web.Storage;
using MicroBootstrap.Web.Storage;

namespace MicroBootstrap.Web.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequestStorage(this IServiceCollection services)
    {
        services.AddScoped<IRequestStorage, RequestStorage>();

        return services;
    }
}
