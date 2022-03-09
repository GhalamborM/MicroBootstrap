using System.Reflection;
using MediatR;
using MicroBootstrap.Abstractions.CQRS.Command;
using MicroBootstrap.Abstractions.CQRS.Query;
using MicroBootstrap.CQRS.Command;
using MicroBootstrap.CQRS.Query;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.CQRS;

public static class Extensions
{
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Assembly[]? assemblies = null,
        Action<IServiceCollection>? doMoreActions = null)
    {
        services.AddMediatR(
            assemblies ?? new[] { Assembly.GetCallingAssembly() },
            x =>
            {
                x.AsScoped();
            });

        services.AddScoped<ICommandProcessor, CommandProcessor>()
            .AddScoped<IQueryProcessor, QueryProcessor>();

        doMoreActions?.Invoke(services);

        return services;
    }
}
